using static Cara.Ui.ConsoleUi;

namespace Cara.Core;

/// Startup boot: scrolling system log + progress bar, then the logotype.
public static class Loader
{
    private static readonly string[] BootLines =
    {
        "loading kernel image ...............",
        "mounting /dev/cara .................",
        "probing hardware bus ...............",
        "init service host: dhcp client .....",
        "init service host: dns client ......",
        "init service host: event log .......",
        "calibrating audio subsystem ........",
        "linking neural lattice .............",
        "decrypting personality matrix ......",
        "loading motto cache ................",
        "loading saying cache ...............",
        "spinning up reminder daemon ........",
        "verifying resource integrity .......",
        "scanning minigame repositories .....",
        "arming haunt triggers ..............",
        "allocating console panes ...........",
        "establishing uplink ................",
        "negotiating session keys ...........",
        "syncing world clock ................",
        "finalizing boot ....................",
    };

    public static void ShowLoading()
    {
        Cls(); HideCur();
        Console.Write(Grn + Bold + "  CARA // SYSTEM BOOT\n" + Rst);
        Console.Write(Gry + "  " + new string('-', 52) + "\n\n" + Rst);

        var rng = new Random();
        int total = BootLines.Length;
        for (int i = 0; i < total; i++)
        {
            int pct = (i + 1) * 100 / total;
            Console.Write(Grn2 + "  " + BootLines[i] + Rst);
            Thread.Sleep(55 + rng.Next(0, 110));
            Console.Write(rng.Next(0, 14) != 0 ? Grn + " [  OK  ]" : Yel + " [ WARN ]");
            Console.Write(Gry + "  " + pct + "%\n" + Rst);
            Thread.Sleep(25);
        }

        Console.Write("\n  " + Grn + "[");
        const int w = 52;
        for (int i = 0; i < w; i++)
        {
            Console.Write("\u2588");
            Thread.Sleep(12 + i / 5);
        }
        Console.Write("]" + Bold + "  READY\n" + Rst);
        Thread.Sleep(400);
        ShowCur();
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
