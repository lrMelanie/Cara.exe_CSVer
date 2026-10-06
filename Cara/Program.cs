using System.Runtime.InteropServices;
using System.Text;
using Cara.Core;

namespace Cara;

internal static class Program
{
    // ANSI colors (virtual terminal)
    private const string Rst = "\x1b[0m";
    private const string Grn = "\x1b[38;5;46m";

    private static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.InputEncoding = Encoding.UTF8;
        EnableVirtualTerminal();
        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        var assistant = new Assistant();

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
                assistant.ProcessScheduleCommand(command["schedule ".Length..]);
                continue;
            }

            switch (command)
            {
                case "WELCOMETOTHEGAME":
                    Cara.Fx.SystemFx.ExecuteWelcomeSequence();
                    assistant.Log("You such a idiot");
                    break;

                case "A.L.I.V.E":
                    Cara.Fx.SystemFx.ExecuteSpectralBroadcast();
                    Console.WriteLine("[System] Audio calibration complete");
                    assistant.Log("Audio calibration complete");
                    break;

                case "HELP-ME":
                    Cara.Fx.SystemFx.TriggerPhantomProtocol();
                    Console.WriteLine("[System] Diagnostic tools activated");
                    assistant.Log("Diagnostic tools activated");
                    break;

                case "D0N'T-L00K-B3H1ND-Y0U":
                    Cara.Fx.SystemFx.InitiateBlackMirror();
                    Console.WriteLine("[System] Environment scan initialized");
                    assistant.Log("Environment scan initialized");
                    break;

                case "fix me":
                    Cara.Fx.SystemFx.FixMe();
                    Console.WriteLine("[System] Guardian protocol executed");
                    assistant.Log("Guardian angel executed");
                    break;

                case "schedule":
                    assistant.ShowScheduleHelp();
                    assistant.Log("Generated schedule help");
                    break;

                case "help":
                    assistant.ShowHelp();
                    assistant.Log("Generated help");
                    break;

                case "motto":
                    assistant.GiveMotto();
                    assistant.Log("Generated motto");
                    break;

                case "say":
                    assistant.Say();
                    assistant.Log("Generated say");
                    break;

                case "minigame":
                    assistant.Log("Launching minigame");
                    Cara.Minigames.MinigameMenu.Run();
                    ShowLogotype();
                    assistant.ShowHelp();
                    break;

                case "exit":
                case "quit":
                    assistant.Log("Exit...");
                    if (assistant.Exit())
                    {
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
        IntPtr handle = GetStdHandle(StdOutputHandle);
        if (GetConsoleMode(handle, out uint mode))
            SetConsoleMode(handle, mode | EnableVirtualTerminalProcessing);
    }
}
