using static Cara.Ui.ConsoleUi;

namespace Cara.Minigames;

/// Reactor Core: SIMPLE, EXTENDED and real-time ReactorRL (city grid) modes.
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
            Console.WriteLine(Row("   [3] REACTOR RL   (real-time, live city grid)"));
            Console.WriteLine(Row("   [4] BACK"));
            Console.WriteLine(Hline("╚", "╝"));
            Console.Write(Rst);
            Console.Write(Grn + "\n  root@cara:~# " + Rst);
            ShowCur();

            string c = Console.ReadLine() ?? "";
            if (c == "1") PlaySimple();
            else if (c == "2") PlayExtended();
            else if (c == "3") PlayReactorRL();
            else if (c == "4") break;
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

    // ReactorRL: real-time core; power city districts, manage rods/coolant, don't melt.
    private void PlayReactorRL()
    {
        string rlPath = Path.Combine(AppContext.BaseDirectory, "resources", "data", "minigame", "reactor", "rl_best.txt");
        int rlBest = 0;
        try { if (File.Exists(rlPath) && int.TryParse(File.ReadAllText(rlPath).Trim(), out int b)) rlBest = b; } catch { }

        string[] cities = { "VARREN CITY", "NULLHAVEN", "GREYMOOR", "ASHFORD-7" };
        string city = cities[_rng.Next(cities.Length)];
        string[] dName = { "Old Town", "Harbor", "Heights", "Foundry", "Hospital" };
        int[] dLoad = { 14, 20, 12, 28, 10 };
        for (int i = 0; i < dLoad.Length; i++) dLoad[i] += _rng.Next(-2, 3);
        bool[] dOn = { true, false, false, false, true };

        const int Rated = 90;
        double temp = 30, coolant = 100, score = 0, simTime = 0;
        int rods = 50;
        bool pumpOn = false, scram = false;

        Cls(); HideCur();
        DateTime last = DateTime.Now;

        while (true)
        {
            DateTime now = DateTime.Now;
            double dt = (now - last).TotalSeconds;
            last = now;
            if (dt > 0.25) dt = 0.25;

            Cara.Fx.Haunt.Maybe(2000);

            double r = 1.0 - rods / 100.0;
            int cap = (int)(Rated * r);
            int load = 0;
            for (int i = 0; i < dOn.Length; i++) if (dOn[i]) load += dLoad[i];
            int served = Math.Min(load, cap);
            int unmet = load - served;

            double heatIn = 0.5 + 0.18 * served + 4.0 * r + 0.8 * Math.Min(unmet, 20);
            double heatOut = (pumpOn && coolant > 0) ? 12.0 : 4.0;
            temp += (heatIn - heatOut) * dt;
            if (temp < 10) temp = 10;

            coolant += (pumpOn && coolant > 0 ? -8.0 : 4.0) * dt;
            if (coolant < 0) coolant = 0;
            if (coolant > 100) coolant = 100;

            score += served * dt;
            simTime += dt;

            if (temp >= 100)
            {
                Cls(); HideCur();
                Console.Write(Red + Bold);
                Console.WriteLine(Hline("╔", "╗"));
                Console.WriteLine(Row("        M E L T D O W N   -   " + city));
                Console.WriteLine(Hline("╚", "╝"));
                Console.Write(Rst);
                for (int k = 5; k >= 1; k--) { Console.Write(Red + "\n        core breach in " + k + Rst + "\n"); Thread.Sleep(320); }
                int fs = (int)score;
                if (fs > rlBest) { try { Directory.CreateDirectory(Path.GetDirectoryName(rlPath)!); File.WriteAllText(rlPath, fs.ToString()); } catch { } }
                Console.Write(Grn + "\n   delivered : " + fs + " MW.s\n   best : " + Math.Max(fs, rlBest) + "\n");
                Pause("press Enter...");
                return;
            }

            RenderRl(city, temp, coolant, rods, r, cap, load, served, unmet, pumpOn, (int)score, (int)simTime, rlBest, dName, dLoad, dOn);

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                char ch = char.ToLowerInvariant(key.KeyChar);
                if (key.Key == ConsoleKey.UpArrow || ch == 'w') rods = Math.Max(0, rods - 5);
                else if (key.Key == ConsoleKey.DownArrow || ch == 's') rods = Math.Min(100, rods + 5);
                else if (ch == 'c') pumpOn = !pumpOn;
                else if (ch == 'q') scram = true;
                else if (ch >= '1' && ch <= '5') dOn[ch - '1'] = !dOn[ch - '1'];
            }
            if (scram) break;

            Thread.Sleep(90);
        }

        Cls(); HideCur();
        int final = (int)score;
        if (final > rlBest) { try { Directory.CreateDirectory(Path.GetDirectoryName(rlPath)!); File.WriteAllText(rlPath, final.ToString()); } catch { } }
        Console.Write(Grn2 + Bold);
        Console.WriteLine(Hline("╔", "╗"));
        Console.WriteLine(Row("        SCRAM - " + city + " OFFLINE, CORE SAFE"));
        Console.WriteLine(Hline("╚", "╝"));
        Console.Write(Rst);
        Console.Write(Grn + "\n   uptime : " + (int)simTime + "s\n   delivered : " + final + " MW.s\n   best : " + Math.Max(final, rlBest) + "\n");
        if (final >= rlBest) Console.Write(Yel + "\n   >> NEW RECORD <<\n" + Rst);
        Pause("press Enter...");
    }

    private static void RenderRl(string city, double temp, double coolant, int rods, double r, int cap,
        int load, int served, int unmet, bool pumpOn, int score, int time, int best,
        string[] dName, int[] dLoad, bool[] dOn)
    {
        int ti = (int)temp, ci = (int)coolant, ri = 100 - rods;
        string tcol = temp < 50 ? Grn : temp < 80 ? Yel : Red;
        string status = temp < 50 ? "NOMINAL" : temp < 80 ? "RISING" : temp < 95 ? "CRITICAL" : "MELTDOWN IMMINENT";

        var sb = new System.Text.StringBuilder();
        sb.Append("\x1b[H");
        sb.Append(Grn).Append(Hline("╔", "╗")).Append("\x1b[K\n");
        sb.Append(Row("  REACTOR RL // " + city + "        [ LIVE ]")).Append("\x1b[K\n");
        sb.Append(Hline("╠", "╣")).Append("\x1b[K\n");
        sb.Append(Row("  TIME " + time + "s    DELIVERED " + score + " MW.s    BEST " + best)).Append("\x1b[K\n");
        sb.Append(Hline("╚", "╝")).Append(Rst).Append("\x1b[K\n");
        sb.Append("\x1b[K\n");
        sb.Append("   CORE  ").Append(Gauge(ti, 100, tcol)).Append("  ").Append(tcol).Append(ti).Append("/100  ").Append(status).Append(Rst).Append("\x1b[K\n");
        sb.Append("   COOL  ").Append(Gauge(ci, 100, Cyan)).Append("  ").Append(Cyan).Append(ci).Append("/100  ").Append(pumpOn ? "PUMP ON" : "pump off").Append(Rst).Append("\x1b[K\n");
        sb.Append("   RODS  ").Append(Gauge(ri, 100, Grn2)).Append("  react ").Append(ri).Append("%  cap ").Append(cap).Append("MW").Append(Rst).Append("\x1b[K\n");
        sb.Append("\x1b[K\n");
        sb.Append("   GRID  demand ").Append(load).Append("MW   supplied ").Append(served).Append("MW");
        if (unmet > 0) sb.Append(Red).Append("   BROWNOUT -").Append(unmet).Append("MW (overheats! up=power)").Append(Rst);
        sb.Append("\x1b[K\n");
        sb.Append("\x1b[K\n");
        sb.Append(Gry).Append("   DISTRICTS").Append(Rst).Append("\x1b[K\n");
        for (int i = 0; i < dName.Length; i++)
            sb.Append("    [").Append(i + 1).Append("] ").Append(dName[i].PadRight(10)).Append(dLoad[i]).Append("MW  ")
              .Append(dOn[i] ? Grn + "ON" : Grn2 + "off").Append(Rst).Append("\x1b[K\n");
        sb.Append("\x1b[K\n");
        sb.Append(Gry).Append("   [1-5] district   C pump   up/down or W/S rods   Q scram").Append(Rst).Append("\x1b[K\n");
        sb.Append("\x1b[J");
        Console.Write(sb.ToString());
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
