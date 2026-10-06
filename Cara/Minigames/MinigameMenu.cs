using static Cara.Ui.ConsoleUi;

namespace Cara.Minigames;

/// The "HACKER TERMINAL" hub that launches the individual minigames.
public static class MinigameMenu
{
    public static void Run()
    {
        EnableVt();
        while (true)
        {
            Cls(); HideCur();
            Console.Write(Grn);
            Console.WriteLine(Hline("╔", "╗"));
            Console.WriteLine(Row("            H A C K E R   T E R M I N A L"));
            Console.WriteLine(Row("            node: cara.exe   clearance: OMEGA"));
            Console.WriteLine(Hline("╠", "╣"));
            Console.WriteLine(Row("   [1] INITIATE CODE RUNNER"));
            Console.WriteLine(Row("   [2] ENTER DICE ARENA"));
            Console.WriteLine(Row("   [3] REACTOR CORE"));
            Console.WriteLine(Row("   [4] RETURN TO MAIN SYSTEM"));
            Console.WriteLine(Hline("╚", "╝"));
            Console.Write(Rst);
            Console.Write(Grn + "\n  root@cara:~# " + Rst);
            ShowCur();

            string c = Console.ReadLine() ?? "";
            if (c == "1") new CodeRunner().Run();
            else if (c == "2") new DiceArena().Run();
            else if (c == "3") new Reactor().Run();
            else if (c == "4") break;
            else { Console.Write(Red + "  invalid selection, operator." + Rst); Thread.Sleep(900); }
        }
        Cls();
    }
}
