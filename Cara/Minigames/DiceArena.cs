using static Cara.Ui.ConsoleUi;

namespace Cara.Minigames;

/// Dice Arena: roll 3 dice each turn, assign each to attack / block / heal
/// against escalating "process" foes. Slice & Dice style.
public sealed class DiceArena
{
    private static readonly string BestPath =
        Path.Combine(AppContext.BaseDirectory, "resources", "data", "minigame", "dicearena", "best.txt");

    private static readonly string[] Foes =
    {
        "WORM.exe", "ROOTKIT", "TROJAN", "DAEMON",
        "RANSOM.dll", "B0TNET", "KEYL0GGER", "WRAITH"
    };

    private readonly Random _rng = new();
    private int _hp, _maxHp, _shield, _wave, _score, _best;

    public void Run()
    {
        EnableVt();
        string cmd;
        do
        {
            _hp = _maxHp = 25;
            _shield = 0;
            _wave = 1;
            _score = 0;
            LoadBest();
            Intro();

            while (_hp > 0)
            {
                string foe = Foes[(_wave - 1) % Foes.Length];
                int eMax = 8 + _wave * 4;
                int eHp = eMax;
                int eAtk = 3 + _wave;
                string note = "a hostile process spawns: " + foe;

                while (eHp > 0 && _hp > 0)
                {
                    _shield = 0;
                    int d1 = _rng.Next(1, 7), d2 = _rng.Next(1, 7), d3 = _rng.Next(1, 7);
                    Render(foe, eHp, eMax, eAtk, d1, d2, d3, note);

                    string a = Console.ReadLine() ?? "";
                    int[] dv = { d1, d2, d3 };
                    int atk = 0, def = 0, heal = 0;
                    for (int i = 0; i < 3; i++)
                    {
                        char c = i < a.Length ? char.ToLowerInvariant(a[i]) : 'a';
                        if (c == 'd') def += dv[i];
                        else if (c == 'h') heal += dv[i];
                        else atk += dv[i];
                    }

                    eHp -= atk;
                    _shield += def;
                    int before = _hp;
                    _hp = Math.Min(_maxHp, _hp + heal);
                    int healed = _hp - before;

                    if (eHp <= 0) { note = foe + " terminated! (" + atk + " dmg)"; break; }

                    string ns = "you: " + atk + " dmg";
                    if (def > 0) ns += ", " + def + " block";
                    if (healed > 0) ns += ", +" + healed + " hp";
                    int dmg = eAtk - _shield;
                    if (dmg < 0) dmg = 0;
                    _hp -= dmg;
                    ns += "   |   " + foe + " hits for " + dmg;
                    note = ns;
                }

                if (_hp > 0)
                {
                    _score++;
                    _wave++;
                    int before = _hp;
                    _hp = Math.Min(_maxHp, _hp + 5);
                    Cls(); HideCur();
                    Console.Write(Grn2 + "\n   [PURGED] " + foe + " cleared.  +" + (_hp - before) + " HP restored.\n" + Rst);
                    Console.Write(Gry + "   next process incoming...\n" + Rst);
                    Console.Write(Grn + "\n  press Enter..." + Rst);
                    ShowCur();
                    Console.ReadLine();
                }
            }

            GameOver();
            SaveBest(_score);
            cmd = (Console.ReadLine() ?? "").ToLowerInvariant();
        }
        while (cmd == "restart" || cmd == "again" || cmd == "r");

        ShowCur();
    }

    private void Intro()
    {
        Cls(); HideCur();
        Console.Write(Grn);
        Console.WriteLine(Hline("╔", "╗"));
        Console.WriteLine(Row("   >> D I C E   A R E N A <<"));
        Console.WriteLine(Row("   roll 3 dice, assign each one, survive the queue"));
        Console.WriteLine(Hline("╠", "╣"));
        Console.WriteLine(Row("   a = attack   spend a die as damage to the process"));
        Console.WriteLine(Row("   d = block    add the die to your shield this turn"));
        Console.WriteLine(Row("   h = heal      restore that many HP (up to max)"));
        Console.WriteLine(Row("   type 3 letters, e.g.  ada   (enter = all attack)"));
        Console.WriteLine(Hline("╚", "╝"));
        Console.Write(Rst);
        Console.Write(Cyan + "\n   The daemons are queued. Clear as many waves as you can.\n" + Rst);
        Console.Write(Grn + "\n  press Enter to start..." + Rst);
        ShowCur();
        Console.ReadLine();
    }

    private void Render(string foe, int eHp, int eMax, int eAtk, int d1, int d2, int d3, string note)
    {
        Cls(); HideCur();
        string top = "  WAVE " + _wave + "    SCORE " + _score + "    BEST " + _best;
        Console.Write(Grn);
        Console.WriteLine(Hline("╔", "╗"));
        Console.WriteLine(Row("  DICE ARENA                          [ COMBAT ]"));
        Console.WriteLine(Hline("╠", "╣"));
        Console.WriteLine(Row(top));
        Console.WriteLine(Hline("╚", "╝"));
        Console.Write(Rst);
        Console.Write("\n   " + Red + foe + Rst + "  (atk " + eAtk + ")\n");
        Console.Write("   FOE  " + Gauge(eHp, eMax, Red) + "  " + Red + eHp + "/" + eMax + Rst + "\n\n");
        Console.Write("   YOU  " + Gauge(_hp, _maxHp, Grn) + "  " + Grn + _hp + "/" + _maxHp + Rst + "   " + Cyan + "shield " + _shield + Rst + "\n\n");
        Console.Write("   DICE   " + Bold + Cyan + "[1] " + d1 + "     [2] " + d2 + "     [3] " + d3 + Rst + "\n");
        Console.Write(Gry + "   a=attack  d=block  h=heal   (3 letters, enter = all attack)\n" + Rst);
        if (note.Length > 0) Console.Write(Yel + "\n   " + note + Rst + "\n");
        Console.Write(Grn + "\n  assign >> " + Rst);
        ShowCur();
    }

    private void GameOver()
    {
        Cls(); HideCur();
        Console.Write(Red + Bold);
        Console.WriteLine(Hline("╔", "╗"));
        Console.WriteLine(Row("           P R O C E S S   K I L L E D"));
        Console.WriteLine(Hline("╚", "╝"));
        Console.Write(Rst);
        Console.Write(Grn + "\n   waves cleared : " + _score + "\n   best on record : " + _best + "\n");
        if (_score > _best) Console.Write(Yel + "\n   >> NEW RECORD <<\n");
        Console.Write(Rst + Gry + "\n   type 'restart' to fight again   |   'exit' to leave\n" + Rst);
        Console.Write(Grn + "\n  root@cara:~# " + Rst);
        ShowCur();
    }

    private void LoadBest()
    {
        _best = 0;
        try
        {
            if (File.Exists(BestPath) && int.TryParse(File.ReadAllText(BestPath).Trim(), out int b))
                _best = b;
        }
        catch { }
    }

    private void SaveBest(int score)
    {
        if (score > _best)
        {
            _best = score;
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(BestPath)!);
                File.WriteAllText(BestPath, score.ToString());
            }
            catch { }
        }
    }
}
