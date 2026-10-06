using static Cara.Ui.ConsoleUi;

namespace Cara.Minigames;

/// Reactor Core minigame: SIMPLE (classic temperature control) and
/// EXTENDED (pressure, coolant, random events, meltdown/scram).
public sealed class Reactor
{
    private static readonly string BestPath =
        Path.Combine(AppContext.BaseDirectory, "resources", "data", "minigame", "reactor", "best.txt");

    private readonly Random _rng = new();
    private int _best;

    public void Run()
    {
        EnableVt();
        while (true)
        {
            Cls(); HideCur();
            Console.Write(Grn);
            Console.WriteLine(Hline("╔", "╗"));
            Console.WriteLine(Row("   >> R E A C T O R   C O R E <<"));
            Console.WriteLine(Hline("╠", "╣"));
            Console.WriteLine(Row("   [1] SIMPLE CORE    (classic reactor)"));
            Console.WriteLine(Row("   [2] EXTENDED CORE (pressure, coolant, events)"));
            Console.WriteLine(Row("   [3] BACK"));
            Console.WriteLine(Hline("╚", "╝"));
            Console.Write(Rst);
            Console.Write(Grn + "\n  root@cara:~# " + Rst);
            ShowCur();

            string c = Console.ReadLine() ?? "";
            if (c == "1") PlaySimple();
            else if (c == "2") PlayExtended();
            else if (c == "3") break;
            else { Console.Write(Red + "  unknown option." + Rst); Thread.Sleep(900); }
        }
        ShowCur();
    }

    private static string HeatColor(int t) => t < 50 ? Grn : t < 80 ? Yel : Red;

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

    private void PlaySimple()
    {
        int temp, timer, loading = 0, points = 0;
        int irp = 0, irc = 0, irw = 0, ira = 0, ira1 = 0, ira2 = 0, iro = 0, awari = 0, nawari = 0;
        string stop = "";

        Cls(); ShowCur();
        Console.Write(Grn + "LOADING...\n" + Rst);
        for (int i = 0; i < 19; i++)
        {
            Console.Write(loading + "%\n");
            if (loading <= 10) { loading++; Thread.Sleep(120); }
            else { loading += 10; Thread.Sleep(60); }
        }
        Console.Write(loading + "%\n");
        loading += 8;
        Console.Write(loading + "%\n");
        Thread.Sleep(800);
        Console.Write("100%\n");
        Thread.Sleep(500);

        Console.Write("-------------------\n");
        Thread.Sleep(400);
        Console.Write("ENTER CURRENT REACTOR TEMPERATURE: ");
        temp = ReadInt(0);

        while (true)
        {
            if (temp < 0) temp = 0;
            Console.Write("\n" + HeatColor(temp) + "CURRENT TEMPERATURE: " + temp + Rst + "\n");
            if (temp == 0) Console.Write("REACTOR IN STANDBY. PLEASE ENGAGE POWER");
            else if (temp < 30) Console.Write(Grn + "TEMPERATURE NOMINAL. NO ACTION NEEDED" + Rst);
            else if (temp < 50) Console.Write("REACTOR ON WATCH. WAKE THE OPERATOR");
            else if (temp < 70) Console.Write(Yel + "TEMPERATURE HIGH. ADD WATER." + Rst);
            else if (temp < 100) Console.Write(Red + "REACTOR NEAR CRITICAL. SHUT DOWN UNTIL COOLED" + Rst);
            else
            {
                Console.Write(Red + Bold + "REACTOR HAS GONE CRITICAL. MELTDOWN IN 45 SECONDS. YOUR LIFELINES: CALL YOUR FAMILY OR START PRAYING" + Rst);
                for (timer = 10; timer >= 0; timer--) { Console.Write("\n" + Red + timer + Rst + "\n"); Thread.Sleep(400); }
                Console.Write(Red + Bold + "GOODBYE" + Rst + "\n");
                break;
            }

            Cara.Fx.Haunt.Maybe(300);
            Console.Write("\nCOMMANDS: \nW - ENGAGE COOLING \nP - INCREASE POWER \nQ - SHUT DOWN \nC - KEEP OBSERVING\n>> ");
            char doing = ReadKey();

            switch (doing)
            {
                case 'p': temp += _rng.Next(1, 12); points += 3; irp++; break;
                case 'w': temp -= _rng.Next(1, 12); points -= 5; irw++; break;
                case 'q': Console.Write("SYSTEM SHUT DOWN\n"); stop = "STOP"; break;
                case 'c': temp += _rng.Next(1, 6); points += 1; irc++; break;
                default:
                    ira++;
                    int szansa = _rng.Next(1, 26);
                    if (szansa > 10)
                    {
                        Console.Write(Red + "EMERGENCY SHUTDOWN FAILED. SYSTEM FAILURE" + Rst);
                        temp += _rng.Next(10, 55); awari += points; points += points; ira1++;
                    }
                    else
                    {
                        Console.Write("EMERGENCY SYSTEM SHUTDOWN");
                        temp -= _rng.Next(10, 55); nawari += points / 2; points -= points / 2; ira2++;
                    }
                    break;
            }

            iro++;

            if (stop == "STOP")
            {
                Console.Write("\n\nYour Points: " + points);
                Console.Write("\nMoves: " + iro);
                Console.Write("\nCooling actions: " + irw + "(" + irw * 5 + ")");
                Console.Write("\nPower increases: " + irp + "(+" + irp * 3 + ")");
                Console.Write("\nObservations: " + irc + "(+" + irc * 1 + ")");
                Console.Write("\nEmergency actions: " + ira);
                Console.Write("\nFailures: " + ira1 + "(+" + awari + ")");
                Console.Write("\nEmergency shutdowns: " + ira2 + "(" + nawari + ")");
                break;
            }
        }

        Pause("press Enter...");
    }

    private void PlayExtended()
    {
        int temp = 20, pressure = 10, coolant = 100, power = 0, turn = 0;
        LoadBest();
        string note = "Reactor online. Keep temp and pressure below 100 and produce power.";

        void Render()
        {
            Cls(); HideCur();
            string st = "  SHIFT " + turn + "     POWER " + power + "     BEST " + _best;
            Console.Write(Grn);
            Console.WriteLine(Hline("╔", "╗"));
            Console.WriteLine(Row("  REACTOR CORE // EXTENDED            [ ONLINE ]"));
            Console.WriteLine(Hline("╠", "╣"));
            Console.WriteLine(Row(st));
            Console.WriteLine(Hline("╚", "╝"));
            Console.Write(Rst);
            Console.Write("\n   TEMP     " + Gauge(temp, 100, HeatColor(temp)) + "  " + HeatColor(temp) + temp + "/100" + Rst + "\n");
            Console.Write("   PRESSURE " + Gauge(pressure, 100, HeatColor(pressure)) + "  " + HeatColor(pressure) + pressure + "/100" + Rst + "\n");
            Console.Write("   COOLANT  " + Gauge(coolant, 100, Cyan) + "  " + Cyan + coolant + "/100" + Rst + "\n\n");
            Console.Write(Gry + "   B=boost power  C=cool  V=vent pressure  H=hold  S=scram\n" + Rst);
            if (note.Length > 0) Console.Write(Yel + "\n   " + note + Rst + "\n");
            Console.Write(Grn + "\n  reactor >> " + Rst);
            ShowCur();
        }

        while (true)
        {
            turn++;
            Cara.Fx.Haunt.Maybe(300);
            temp += _rng.Next(0, 3);
            int ev = _rng.Next(1, 101);
            if (ev <= 15) { int s = _rng.Next(8, 16); temp += s; pressure += 6; note = "EVENT: power surge! temp +" + s + ", pressure rising."; }
            else if (ev <= 30) { int leak = _rng.Next(10, 20); coolant -= leak; if (coolant < 0) coolant = 0; temp += 5; note = "EVENT: coolant leak (-" + leak + ")."; }
            else if (ev <= 40) { if (temp < 50 && pressure < 60) { power += 15; note = "EVENT: inspection passed (+15 power)."; } else note = "EVENT: inspection failed (reactor too stressed)."; }
            else if (turn > 1) note = "Reactor stable.";

            if (coolant > 100) coolant = 100;
            if (temp < 0) temp = 0;
            if (pressure < 0) pressure = 0;

            Render();
            char c = ReadKey();

            if (c == 'b') { int g = _rng.Next(8, 16); temp += g; pressure += 5; power += 10; note = "BOOST: +10 power, temp +" + g + "."; }
            else if (c == 'c') { if (coolant >= 15) { coolant -= 15; int cd = _rng.Next(12, 21); temp -= cd; if (temp < 0) temp = 0; note = "COOLING: temp -" + cd + ", coolant -15."; } else note = "No coolant! Cannot cool."; }
            else if (c == 'v') { int vd = _rng.Next(20, 31); pressure -= vd; if (pressure < 0) pressure = 0; temp -= 5; if (temp < 0) temp = 0; power -= 3; if (power < 0) power = 0; note = "VENT: pressure -" + vd + ", -3 power."; }
            else if (c == 'h') { power += 2; coolant += 5; if (coolant > 100) coolant = 100; temp += _rng.Next(1, 4); note = "HOLD: +2 power, coolant regenerates."; }
            else if (c == 's') break;
            else note = "Unknown command. Reactor waiting.";

            if (temp >= 100 || pressure >= 100)
            {
                Cls(); HideCur();
                Console.Write(Red + Bold);
                Console.WriteLine(Hline("╔", "╗"));
                Console.WriteLine(Row("              M E L T D O W N"));
                Console.WriteLine(Hline("╚", "╝"));
                Console.Write(Rst);
                for (int k = 5; k >= 1; k--) { Console.Write(Red + "\n        core overheating... " + k + Rst + "\n"); Thread.Sleep(350); }
                Console.Write(Red + Bold + "\n   REACTOR LOST.\n" + Rst);
                Console.Write(Grn + "\n   power produced : " + power + "\n   best : " + _best + "\n" + Rst);
                SaveBest(power);
                Pause("press Enter...");
                return;
            }
        }

        Cls(); HideCur();
        Console.Write(Grn2 + Bold);
        Console.WriteLine(Hline("╔", "╗"));
        Console.WriteLine(Row("        SCRAM - SAFE SHUTDOWN"));
        Console.WriteLine(Hline("╚", "╝"));
        Console.Write(Rst);
        Console.Write(Grn + "\n   shifts survived : " + turn + "\n   power produced : " + power + "\n   best : " + _best + "\n");
        SaveBest(power);
        if (power >= _best) Console.Write(Yel + "\n   >> NEW RECORD <<\n" + Rst);
        Pause("press Enter...");
    }
}
