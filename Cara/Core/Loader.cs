using System.Diagnostics;
using System.Globalization;
using static Cara.Ui.ConsoleUi;

namespace Cara.Core;

/// Startup boot: CRT power-on, POST, timestamped boot log, progress, READY.
public static class Loader
{
    private const string D0 = "\x1b[38;5;52m", D1 = "\x1b[38;5;88m", D2 = "\x1b[38;5;124m",
        D3 = "\x1b[38;5;160m", D4 = "\x1b[38;5;196m", BgBar = "\x1b[48;5;160m",
        Ink = "\x1b[38;5;16m", Wh = "\x1b[38;5;231m", G1 = "\x1b[38;5;238m", G2 = "\x1b[38;5;244m",
        Ok = "\x1b[38;5;34m", Amb = "\x1b[38;5;214m";

    private static readonly string[] BootLines =
    {
        "loading kernel image ...............",
        "mounting /dev/cara .................",
        "probing hardware bus ...............",
        "init service host: dhcp client .....",
        "init service host: dns client ......",
        "calibrating audio subsystem ........",
        "linking neural lattice .............",
        "decrypting personality matrix ......",
        "loading motto cache ................",
        "spinning up reminder daemon ........",
        "verifying resource integrity .......",
        "scanning minigame repositories .....",
        "arming haunt triggers ..............",
        "allocating console panes ...........",
        "establishing uplink ................",
        "syncing world clock ................",
        "finalizing boot ....................",
    };

    public static void ShowLoading()
    {
        bool plain = false;
        int ww = 80, wh = 25;
        try { ww = Console.WindowWidth; wh = Console.WindowHeight; plain = Console.IsOutputRedirected; } catch { }
        int W = Math.Clamp(ww - 4, 40, 72);
        var rng = new Random();
        var clock = Stopwatch.StartNew();

        try
        {
            if (plain)
            {
                foreach (var ln in BootLines) Console.WriteLine("  " + ln + "   [  OK  ]");
                Console.WriteLine("  READY  cara.os online");
                return;
            }

            Cls(); HideCur();
            int mid = Math.Max(1, wh / 2);
            for (int i = 1; i <= 6; i++)
            {
                int len = W * i / 6;
                Console.Write("\x1b[" + mid + ";3H" + Wh + new string('\u2500', len) + Rst);
                Thread.Sleep(22);
            }
            Console.Write("\x1b[" + mid + ";3H" + D3 + new string('\u2588', W) + Rst);
            Thread.Sleep(40);
            Cls();

            // title bar
            string title = " CARA.OS // SYSTEM BOOT";
            string right = "BIOS 2.6 ";
            string bar = title.PadRight(W - right.Length - 3);
            Console.Write("  " + BgBar + Ink + Bold + bar + right + Rst
                + "\x1b[38;5;196;48;5;52m[x]" + Rst + "\n");

            // POST mem count-up
            Console.Write("  " + G2 + "CARA BIOS 2.6    LATTICE x8    MEM " + Rst);
            int col = 2 + "CARA BIOS 2.6    LATTICE x8    MEM ".Length + 1;
            for (int m = 0; m <= 65536; m += 4096)
            {
                Console.Write("\x1b[" + col + "G" + D3 + m.ToString(CultureInfo.InvariantCulture).PadLeft(5) + "K" + Rst);
                Thread.Sleep(18);
            }
            Console.Write(Ok + " OK\n" + Rst);
            Console.Write("  " + D1 + new string('\u2500', W) + Rst + "\n");

            // boot lines
            var spin = "|/-\\";
            for (int i = 0; i < BootLines.Length; i++)
            {
                string ln = BootLines[i];
                int dot = ln.IndexOf(" .", StringComparison.Ordinal);
                string name = dot < 0 ? ln : ln[..dot];
                string dots = dot < 0 ? "" : ln[dot..];
                Console.Write("  " + G1 + "[" + (clock.ElapsedMilliseconds / 1000.0).ToString("0.000", CultureInfo.InvariantCulture).PadLeft(6) + "]  "
                    + D3 + name + D1 + dots + Rst + "   ");
                int scol = 2 + 1 + 8 + 2 + ln.Length + 3;
                int wait = 55 + rng.Next(110);
                for (int t = 0; t < wait; t += 35)
                {
                    Console.Write("\x1b[" + scol + "G" + D2 + "[ " + spin[(t / 35) % 4] + "  ]" + Rst);
                    Thread.Sleep(35);
                }
                bool uplinkGag = name.Contains("establishing uplink") && rng.Next(2) == 0;
                if (uplinkGag)
                {
                    Console.Write("\x1b[" + scol + "G" + Wh + BgBar + "[ FAIL ]" + Rst + "\n");
                    Thread.Sleep(180);
                    Console.Write("  " + G1 + "[" + (clock.ElapsedMilliseconds / 1000.0).ToString("0.000", CultureInfo.InvariantCulture).PadLeft(6) + "]  "
                        + D3 + "retrying uplink (2/3)" + D1 + " .............." + Rst + "   "
                        + D1 + "[" + Ok + "  OK  " + D1 + "]" + Rst + "\n");
                }
                else if (rng.Next(14) == 0)
                    Console.Write("\x1b[" + scol + "G" + D1 + "[" + Amb + " WARN " + D1 + "]" + Rst + "\n");
                else
                    Console.Write("\x1b[" + scol + "G" + D1 + "[" + Ok + "  OK  " + D1 + "]" + Rst + "\n");
            }

            Console.Write("  " + D1 + new string('\u2500', W) + Rst + "\n");

            // progress bar
            int bw = Math.Clamp(W - 22, 20, 48);
            for (int i = 0; i <= bw; i++)
            {
                int pct = i * 100 / bw;
                var sb = new System.Text.StringBuilder("\r  " + D2 + "LOADING  " + D4 + "[");
                for (int k = 0; k < bw; k++) sb.Append(k < i ? (k == i - 1 ? Wh + "\u2588" + D4 : "\u2588") : D0 + "\u2591" + D4);
                sb.Append(D4 + "]  " + Wh + pct.ToString(CultureInfo.InvariantCulture).PadLeft(3) + "%" + Rst);
                Console.Write(sb.ToString());
                Thread.Sleep(10 + i / 6);
            }

            // READY (RGB split)
            Console.Write("\n\n");
            foreach (int o in new[] { 2, 1 })
            {
                Console.Write("  " + Cyan + "READY" + "\r  " + "\x1b[" + o + "C" + Red + "READY" + "\r  " + Wh + "READY" + Rst);
                Thread.Sleep(50);
            }
            Console.Write("\r  " + BgBar + Ink + Bold + " READY " + Rst + G2 + "  cara.os online" + Rst + "\n");
            Cara.Fx.Panes.Glitch(350);
            Thread.Sleep(400);
        }
        catch { }
        finally { ShowCur(); }
    }

    public static void ShowLogotype()
    {
        Console.WriteLine(Grn +
            "----------------------------------\n" +
            "  _____          _____             \n" +
            " / ____|   /\\   |  __ \\     /\\     \n" +
            "| |       /  \\  | |__) |   /  \\    \n" +
            "| |      / /\\ \\ |  _  /   / /\\ \\   \n" +
            "| |____ / ____ \\| | \\ \\  / ____ \\  \n" +
            " \\_____/_/    \\_\\_|  \\_\\/_/    \\_\\ \n" +
            "----------------------------------" + Rst);
    }
}
