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

        var assistant = new Assistant();

        ShowLogotype();
        Thread.Sleep(400);
        assistant.PrintSlowly("Hello! I'm Cara and I am your virtual assistant.\n");
        Thread.Sleep(400);
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
                    Console.WriteLine("[minigames not ported yet]");
                    assistant.Log("Minigame requested (stub)");
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
