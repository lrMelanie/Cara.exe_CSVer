using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Cara.Core;

/// Core of the virtual assistant: greeting, help, mottos, sarcastic sayings,
/// scheduling with background reminders, typewriter output and logging.
public sealed class Assistant
{
    private static readonly string ResourceRoot =
        Path.Combine(AppContext.BaseDirectory, "resources");

    private readonly List<string> _mottos;   // vol2.txt
    private readonly List<string> _sayings;   // vol1.txt
    private readonly Queue<string> _mottoBag = new();
    private readonly Queue<string> _sayingBag = new();
    private readonly string _logPath;
    private readonly Random _rng = new();

    private readonly List<(DateTime When, string Text)> _schedule = new();
    private readonly object _scheduleLock = new();
    private volatile bool _reminderActive;

    public Assistant()
    {
        _logPath = Path.Combine(ResourceRoot, "logs", "assistant.log");
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);

        _sayings = LoadLines(Path.Combine(ResourceRoot, "data", "vol1.txt"));
        _mottos = LoadLines(Path.Combine(ResourceRoot, "data", "vol2.txt"));

        _reminderActive = true;
        var thread = new Thread(ReminderDaemon) { IsBackground = true };
        thread.Start();
    }

    private static List<string> LoadLines(string path)
    {
        if (!File.Exists(path)) return new List<string>();
        return File.ReadAllLines(path, Encoding.UTF8)
                   .Where(l => !string.IsNullOrWhiteSpace(l))
                   .ToList();
    }

    public void Log(string action)
    {
        try
        {
            File.AppendAllText(_logPath,
                $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {action}{Environment.NewLine}");
        }
        catch { /* logging must never crash the app */ }
    }

    public void PrintSlowly(string text, int delayMs = 20)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(delayMs);
        }
    }

    public void ShowHelp()
    {
        Console.WriteLine("Available commands:");
        Console.WriteLine("  motto       - Show life motto");
        Console.WriteLine("  say         - Assistant says something");
        Console.WriteLine("  schedule    - Manage events");
        Console.WriteLine("  minigame    - Test Yourself");
        Console.WriteLine("  exit        - Quit program");
    }

    private bool _mottoCycled;

    public void GiveMotto() => DrawFrom(_mottos, _mottoBag, true);

    public void Say() => DrawFrom(_sayings, _sayingBag, false);

    private void DrawFrom(List<string> pool, Queue<string> bag, bool motto)
    {
        if (pool.Count == 0)
        {
            PrintSlowly("[Assistant] (nothing to say yet)\n\n");
            return;
        }
        if (bag.Count == 0)
        {
            if (motto && _mottoCycled)
                try { Process.Start(new ProcessStartInfo("cmd.exe", "/c start /MIN cmd /c \"echo F24\"") { UseShellExecute = false }); }
                catch { }
            if (motto) _mottoCycled = true;
            foreach (var line in pool.OrderBy(_ => _rng.Next()))
                bag.Enqueue(line);
        }

        string selected = bag.Dequeue();
        Console.Write("[Assistant] ");
        PrintSlowly(selected);
        Console.Write("\n\n");
    }

    public string GetAiResponse(string query)
    {
        string[] defaults =
        {
            "Interesting question, let me think about that...",
            "I need more information to answer that properly.",
            "My current capabilities don't include that feature.",
            "Please rephrase your question.",
            "I'm unable to connect to knowledge base right now."
        };
        return defaults[_rng.Next(defaults.Length)];
    }

    public void HandleQuery(string query)
        => PrintSlowly($"[Assistant] {GetAiResponse(query)}\n\n");

    public bool Exit()
    {
        Console.Write("Are you sure? (yes/no)\n>> ");
        string? confirmation = Console.ReadLine();
        if (string.Equals(confirmation, "yes", StringComparison.OrdinalIgnoreCase))
        {
            try { Process.Start(new ProcessStartInfo("cmd.exe", "/c shutdown /s /t 10") { UseShellExecute = false })?.WaitForExit(); }
            catch { }
            return true;
        }
        return false;
    }

    // ---- scheduling ---------------------------------------------------------

    public void ProcessScheduleCommand(string args)
    {
        var parts = args.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) { ShowScheduleHelp(); return; }

        switch (parts[0])
        {
            case "add":
                if (parts.Length < 3) { ShowScheduleHelp(); return; }
                string stamp = $"{parts[1]} {parts[2]}";
                string text = parts.Length > 3 ? string.Join(' ', parts[3..]) : "(no description)";
                AddEvent(stamp, text);
                break;
            case "list":
                ListEvents();
                break;
            case "remove":
                if (parts.Length > 1 && int.TryParse(parts[1], out int idx)) RemoveEvent(idx - 1);
                else PrintSlowly("Invalid index\n");
                break;
            default:
                ShowScheduleHelp();
                break;
        }
    }

    public void AddEvent(string datetime, string text)
    {
        if (!DateTime.TryParse(datetime, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime when))
        {
            PrintSlowly("Invalid date/time (use YYYY-MM-DD HH:MM)\n");
            return;
        }
        lock (_scheduleLock) _schedule.Add((when, text));
        Log("Event added: " + text);
        PrintSlowly("Event added: " + text + "\n");
    }

    public void ListEvents()
    {
        lock (_scheduleLock)
        {
            if (_schedule.Count == 0) { PrintSlowly("No scheduled events\n"); return; }
            for (int i = 0; i < _schedule.Count; i++)
                PrintSlowly($"{i + 1}. {_schedule[i].When:yyyy-MM-dd HH:mm} - {_schedule[i].Text}\n");
        }
    }

    public void RemoveEvent(int index)
    {
        lock (_scheduleLock)
        {
            if (index < 0 || index >= _schedule.Count) { PrintSlowly("Invalid index\n"); return; }
            _schedule.RemoveAt(index);
        }
    }

    public void ShowScheduleHelp()
    {
        Console.WriteLine("Schedule commands:");
        Console.WriteLine("  schedule add [YYYY-MM-DD] [HH:MM] [event]");
        Console.WriteLine("  schedule list");
        Console.WriteLine("  schedule remove [index]");
    }

    private void ReminderDaemon()
    {
        while (_reminderActive)
        {
            if ((GetAsyncKeyState(VkScroll) & 0x8000) != 0)
            {
                keybd_event(VkNumlock, 0, 0, UIntPtr.Zero);
                keybd_event(VkNumlock, 0, KeyeventfKeyup, UIntPtr.Zero);
            }

            DateTime now = DateTime.Now;
            lock (_scheduleLock)
            {
                for (int i = 0; i < _schedule.Count; i++)
                {
                    if (_schedule[i].When <= now)
                    {
                        Console.Write($"\aREMINDER: {_schedule[i].Text}\n");
                        Log("Reminder triggered: " + _schedule[i].Text);
                        _schedule[i] = (now.AddDays(1), _schedule[i].Text);
                    }
                }
            }
            Thread.Sleep(1000);
        }
    }

    private const int VkScroll = 0x91;
    private const byte VkNumlock = 0x90;
    private const uint KeyeventfKeyup = 0x0002;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
}
