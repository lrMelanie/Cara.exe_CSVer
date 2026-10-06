using System.Text;

namespace Cara.Core;

/// Core of the virtual assistant: greeting, help, mottos, sarcastic sayings,
/// typewriter output and logging. System features and minigames are added on top.
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

    public Assistant()
    {
        _logPath = Path.Combine(ResourceRoot, "logs", "assistant.log");
        Directory.CreateDirectory(Path.GetDirectoryName(_logPath)!);

        _sayings = LoadLines(Path.Combine(ResourceRoot, "data", "vol1.txt"));
        _mottos = LoadLines(Path.Combine(ResourceRoot, "data", "vol2.txt"));
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
        Console.WriteLine("  minigame    - Test Yourself");
        Console.WriteLine("  exit        - Quit program");
    }

    public void GiveMotto() => DrawFrom(_mottos, _mottoBag);

    public void Say() => DrawFrom(_sayings, _sayingBag);

    // Draws a random line without repeating until the whole pool is used up.
    private void DrawFrom(List<string> pool, Queue<string> bag)
    {
        if (pool.Count == 0)
        {
            PrintSlowly("[Assistant] (nothing to say yet)\n\n");
            return;
        }
        if (bag.Count == 0)
            foreach (var line in pool.OrderBy(_ => _rng.Next()))
                bag.Enqueue(line);

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

    // Returns true when the program should quit.
    public bool Exit()
    {
        Console.Write("Are you sure? (yes/no)\n>> ");
        string? confirmation = Console.ReadLine();
        return string.Equals(confirmation, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
