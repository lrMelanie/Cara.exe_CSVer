using System.Runtime.InteropServices;
using System.Text;
using Cara.Core;

namespace Cara;

internal static class Program
{
    // ANSI colors (virtual terminal)
    private const string Rst = "\x1b[0m";
    private const string Grn = "\x1b[38;5;46m";

    private static void Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        try { Console.InputEncoding = Encoding.UTF8; } catch { }
        EnableVirtualTerminal();

        // satellite window (--pane <slot> <dir> <parentPid>): renders from files
        if (args.Length >= 2 && args[0] == "--pane")
        {
            string slot = args[1];
            string dir = args.Length > 2 ? args[2] : "";
            int ppid = args.Length > 3 && int.TryParse(args[3], out int pp) ? pp : 0;
            Cara.Fx.Panes.RunPane(slot, dir, ppid);
            return;
        }

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        var assistant = new Assistant();

        Cara.Fx.Panes.Start();
        Loader.ShowLoading();
        Console.Clear();
        ShowLogotype();
        Thread.Sleep(600);
        assistant.PrintSlowly("Hello! I'm Cara and I am your virtual assistant.\n");
        Thread.Sleep(600);
        assistant.ShowHelp();

        while (true)
        {
            Console.Write(">> ");
            string command = Console.ReadLine() ?? string.Empty;

            if (command.StartsWith("schedule ", StringComparison.Ordinal))
            {
                Cara.Fx.Panes.Time();
                assistant.ProcessScheduleCommand(command["schedule ".Length..]);
                continue;
            }

            if (command.StartsWith("calc ", StringComparison.Ordinal))
            { Cara.Fx.Panes.Calc(); Tools.Calc(command["calc ".Length..]); assistant.Log("calc"); continue; }

            if (command.StartsWith("timer ", StringComparison.Ordinal))
            { Cara.Fx.Panes.Timer(); Tools.Timer(command["timer ".Length..]); assistant.Log("timer"); continue; }

            if (command.StartsWith("base ", StringComparison.Ordinal))
            { Cara.Fx.Panes.Secure("base"); Tools.BaseConv(command["base ".Length..]); assistant.Log("base"); continue; }

            if (command.StartsWith("genpass ", StringComparison.Ordinal))
            { Cara.Fx.Panes.Secure("genpass"); Tools.GenPass(command["genpass ".Length..]); assistant.Log("genpass"); continue; }

            if (command.Equals("note", StringComparison.Ordinal) || command.Equals("notes", StringComparison.Ordinal)
                || command.StartsWith("note ", StringComparison.Ordinal) || command.StartsWith("notes ", StringComparison.Ordinal))
            {
                Cara.Fx.Panes.Archive();
                int sp = command.IndexOf(' ');
                Tools.Notes(sp < 0 ? "" : command[(sp + 1)..]);
                assistant.Log("notes");
                continue;
            }

            switch (command)
            {
                case "WELCOMETOTHEGAME":
                    Cara.Fx.Panes.Ritual();
                    Cara.Fx.SystemFx.ExecuteWelcomeSequence();
                    assistant.Log("You such a idiot");
                    break;

                case "A.L.I.V.E":
                    Cara.Fx.Panes.Watch();
                    Cara.Fx.SystemFx.ExecuteSpectralBroadcast();
                    Console.WriteLine("[System] Audio calibration complete");
                    assistant.Log("Audio calibration complete");
                    break;

                case "HELP-ME":
                    Cara.Fx.Panes.Breach();
                    Cara.Fx.SystemFx.TriggerPhantomProtocol();
                    Console.WriteLine("[System] Diagnostic tools activated");
                    assistant.Log("Diagnostic tools activated");
                    break;

                case "D0N'T-L00K-B3H1ND-Y0U":
                    Cara.Fx.Panes.Signal();
                    Cara.Fx.SystemFx.InitiateBlackMirror();
                    Console.WriteLine("[System] Environment scan initialized");
                    assistant.Log("Environment scan initialized");
                    break;

                case "fix me":
                    Cara.Fx.Panes.Recover();
                    Cara.Fx.SystemFx.FixMe();
                    Console.WriteLine("[System] Guardian protocol executed");
                    assistant.Log("Guardian angel executed");
                    break;

                case "schedule":
                    Cara.Fx.Panes.Time();
                    assistant.ShowScheduleHelp();
                    assistant.Log("Generated schedule help");
                    break;

                case "help":
                    Cara.Fx.Panes.Index();
                    assistant.ShowHelp();
                    assistant.Log("Generated help");
                    break;

                case "motto":
                    Cara.Fx.Panes.Voice();
                    assistant.GiveMotto();
                    assistant.Log("Generated motto");
                    break;

                case "say":
                    Cara.Fx.Panes.Voice();
                    assistant.Say();
                    assistant.Log("Generated say");
                    break;

                case "minigame":
                    assistant.Log("Launching minigame");
                    Cara.Minigames.MinigameMenu.Run();
                    Cara.Fx.Panes.Idle();
                    ShowLogotype();
                    assistant.ShowHelp();
                    break;

                case "sysinfo":
                    Cara.Fx.Panes.Host();
                    Tools.SysInfo();
                    assistant.Log("sysinfo");
                    break;

                case "clock":
                case "worldclock":
                    Cara.Fx.Panes.Time();
                    Tools.Clock();
                    assistant.Log("clock");
                    break;

                case "genpass":
                    Cara.Fx.Panes.Secure("genpass");
                    Tools.GenPass("");
                    assistant.Log("genpass");
                    break;

                case "hud":
                case "desktop":
                    assistant.Log("Launching desktop");
                    Cara.Ui.Hud.Run();
                    ShowLogotype();
                    break;

                case "exit":
                case "quit":
                    assistant.Log("Exit...");
                    if (assistant.Exit())
                    {
                        Cara.Fx.Panes.Stop();
                        Console.WriteLine("Goodbye!");
                        return;
                    }
                    break;

                default:
                    if (!Cara.Fx.Haunt.TryCode(command))
                        assistant.HandleQuery(command);
                    break;
            }
        }
    }

    private static void ShowLogotype()
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

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    private const int StdOutputHandle = -11;
    private const uint EnableVirtualTerminalProcessing = 0x0004;

    private static void EnableVirtualTerminal()
    {
        if (!OperatingSystem.IsWindows()) return;   // Linux terminals handle ANSI natively
        try
        {
            IntPtr handle = GetStdHandle(StdOutputHandle);
            if (GetConsoleMode(handle, out uint mode))
                SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing);
        }
        catch { }
    }
}
