using System.Diagnostics;
using System.Runtime.InteropServices;
using static Cara.Ui.ConsoleUi;

namespace Cara.Minigames;

/// Code Runner: transcribe lines of source under a shrinking time trace,
/// escalating difficulty, with selectable repositories and a hidden mode.
public sealed class CodeRunner
{
    private static readonly string RepDir =
        Path.Combine(AppContext.BaseDirectory, "resources", "data", "minigame", "coderunner");

    private int _points, _highScore, _linesToType = 2;
    private List<string> _repositoryFiles;
    private readonly Random _rng = new();

    public CodeRunner()
    {
        _repositoryFiles = LoadActiveRepositories();
        if ((GetAsyncKeyState(VkF12) & 0x8000) != 0)
            _repositoryFiles.AddRange(new[] { Og(1), Og(2), Og(3), Og(4) });
        LoadHighScore();
    }

    public void Run()
    {
        EnableVt();
        ShowMenu();
    }

    private static string Rep(string f) => Path.Combine(RepDir, f);
    private static string Og(int n) => Path.Combine(RepDir, $"og_repository{n}.txt");

    private static List<string> LoadActiveRepositories()
    {
        var active = new List<string>();
        try
        {
            string path = Path.Combine(RepDir, "options.txt");
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                    switch (line.Trim())
                    {
                        case "easy": active.Add(Rep("repository3.txt")); active.Add(Rep("repository6.txt")); break;
                        case "medium": active.Add(Rep("repository1.txt")); active.Add(Rep("repository5.txt")); break;
                        case "hard": active.Add(Rep("repository2.txt")); active.Add(Rep("repository7.txt")); break;
                        case "veryhard": active.Add(Rep("repository4.txt")); active.Add(Rep("repository8.txt")); break;
                    }
        }
        catch { }

        if (active.Count == 0) { active.Add(Rep("repository1.txt")); active.Add(Rep("repository5.txt")); }
        return active;
    }

    private void LoadHighScore()
    {
        _highScore = 0;
        try
        {
            string p = Path.Combine(RepDir, "hscore.txt");
            if (File.Exists(p) && int.TryParse(File.ReadAllText(p).Trim(), out int h)) _highScore = h;
        }
        catch { }
    }

    private void SaveHighScore()
    {
        if (_points > _highScore)
        {
            try
            {
                Directory.CreateDirectory(RepDir);
                File.WriteAllText(Path.Combine(RepDir, "hscore.txt"), _points.ToString());
            }
            catch { }
        }
    }

    private List<string> LoadRepositoryFile(string path)
    {
        var lines = new List<string>();
        try
        {
            if (File.Exists(path))
                foreach (var l in File.ReadAllLines(path))
                    if (l.Length > 0) lines.Add(l);
        }
        catch { }
        return lines;
    }

    private List<string> GetConsecutiveLines(List<string> lines, int count)
    {
        if (lines.Count == 0) return new List<string>();
        if (lines.Count < count) count = lines.Count;
        int start = _rng.Next(0, lines.Count - count + 1);
        return lines.GetRange(start, count);
    }

    private static string TimeBar(int rem, int max)
    {
        double f = max > 0 ? (double)rem / max : 0;
        string col = f > 0.5 ? Grn : f > 0.25 ? Yel : Red;
        return Gauge(rem, max, col);
    }

    private void BootSequence()
    {
        Cls(); HideCur();
        string[] steps =
        {
            "establishing uplink ........",
            "spoofing MAC + route .......",
            "mounting /dev/payload ......",
            "decrypting source stream ..."
        };
        Console.Write(Grn + "\n");
        foreach (var s in steps)
        {
            Console.Write("   " + s);
            Thread.Sleep(130);
            Console.Write(" " + Grn2 + "[ OK ]" + Grn + "\n");
            Thread.Sleep(80);
        }
        Console.Write(Cyan + "\n   >> TRANSCRIBE THE INCOMING SOURCE TO HOLD THE CONNECTION <<\n" + Rst);
        Thread.Sleep(650);
    }

    private void Glitch()
    {
        Console.Write(Red);
        for (int r = 0; r < 3; r++)
        {
            Console.Write("   ");
            for (int c = 0; c < 44; c++) Console.Write((char)_rng.Next(33, 127));
            Console.Write("\n");
        }
        Console.Write(Rst);
        Thread.Sleep(45);
    }

    private void RenderGame(int rem, int max, List<string> lines, int cur)
    {
        Cls(); HideCur();
        string info = "  PTS " + _points + "    LVL " + _linesToType + "    BEST " + _highScore;
        Console.Write(Grn);
        Console.WriteLine(Hline("╔", "╗"));
        Console.WriteLine(Row("  CARA://CODE_RUNNER              [ UPLINK ACTIVE ]"));
        Console.WriteLine(Hline("╠", "╣"));
        Console.WriteLine(Row(info));
        Console.WriteLine(Hline("╚", "╝"));
        Console.Write(Rst);
        Console.Write("\n   TRACE " + TimeBar(rem, max) + "  " + Grn + rem + "s" + Rst + "\n\n");
        for (int j = 0; j < lines.Count; j++)
        {
            if (j < cur) Console.WriteLine(Grn2 + "   [OK] " + lines[j] + Rst);
            else if (j == cur) Console.WriteLine(Bold + Cyan + "   >>>  " + lines[j] + Rst);
            else Console.WriteLine(Gry + "    ..  " + lines[j] + Rst);
        }
        Console.Write(Grn + "\n  root@cara:~# " + Rst);
        ShowCur();
    }

    private void DisplayGameOver()
    {
        Cls(); HideCur();
        Console.Write(Red + Bold);
        Console.WriteLine(Hline("╔", "╗"));
        Console.WriteLine(Row("           C O N N E C T I O N   T R A C E D"));
        Console.WriteLine(Hline("╚", "╝"));
        Console.Write(Rst);
        Console.Write(Grn + "\n   payload segments accepted : " + _points + "\n   best on record            : " + _highScore + "\n");
        if (_points > _highScore) Console.Write(Yel + "\n   >> NEW RECORD - you slipped the trace <<\n");
        Console.Write(Rst + Gry + "\n   type 'restart' to re-run   |   'exit' to disconnect\n" + Rst);
        Console.Write(Grn + "\n  root@cara:~# " + Rst);
        ShowCur();
    }

    private void PlayGame()
    {
        const int MaxS = 90;
        string cmd;
        while (true)
        {
            _points = 0;
            _linesToType = 2;
            BootSequence();
            DateTime endTime = DateTime.Now.AddSeconds(MaxS);

            if (Environment.TickCount64 % 137 == 0) TryOpenWebcam();

            while (true)
            {
                if (_rng.Next(0, 13) == 0) Glitch();
                var lines = LoadRepositoryFile(_repositoryFiles[_rng.Next(_repositoryFiles.Count)]);
                var current = GetConsecutiveLines(lines, _linesToType);

                if (current.Count == 0)
                {
                    Console.Write(Red + "  [FATAL] corrupt source node\n" + Rst);
                    Thread.Sleep(1200);
                    return;
                }

                for (int i = 0; i < current.Count;)
                {
                    DateTime now = DateTime.Now;
                    if (now >= endTime) { SaveHighScore(); goto gameOver; }

                    int remaining = (int)(endTime - now).TotalSeconds;
                    RenderGame(remaining, MaxS, current, i);

                    string input = Console.ReadLine() ?? "";
                    if (input == current[i])
                    {
                        _points++;
                        i++;
                        DateTime cap = DateTime.Now.AddSeconds(MaxS);
                        endTime = endTime.AddSeconds(4);
                        if (endTime > cap) endTime = cap;
                        if (_points % 7 == 0) _linesToType++;
                        Console.Write(Grn2 + "   [GRANTED] segment accepted  +4s" + Rst);
                        Thread.Sleep(180);
                    }
                    else
                    {
                        Console.Write(Red + "   [DENIED] checksum mismatch - retry" + Rst);
                        Thread.Sleep(650);
                    }
                }
            }

        gameOver:
            SaveHighScore();
            DisplayGameOver();
            cmd = (Console.ReadLine() ?? "").ToLowerInvariant();
            if (cmd == "exit") { ShowCur(); return; }
            if (cmd == "restart" || cmd == "play again" || cmd == "again") continue;
            Console.Write(Red + "  unknown command - 'restart' or 'exit'\n" + Rst);
            Thread.Sleep(1000);
        }
    }

    private void ShowMenu()
    {
        while (true)
        {
            Cls(); HideCur();
            Console.Write(Grn);
            Console.WriteLine(Hline("╔", "╗"));
            Console.WriteLine(Row("   >> C O D E   R U N N E R <<"));
            Console.WriteLine(Row("   transcribe the stream before the trace lands"));
            Console.WriteLine(Hline("╠", "╣"));
            Console.WriteLine(Row("   [1] INITIATE RUN"));
            Console.WriteLine(Row("   [2] DIFFICULTY MATRIX"));
            Console.WriteLine(Row("   [3] DISCONNECT"));
            Console.WriteLine(Hline("╚", "╝"));
            Console.Write(Rst);
            Console.Write(Grn + "\n  root@cara:~# " + Rst);
            ShowCur();

            string c = Console.ReadLine() ?? "";
            string cl = c.ToLowerInvariant();

            if (c == "1" || cl == "start" || cl == "start game") PlayGame();
            else if (c == "2" || cl == "options" || cl == "difficulty options")
            {
                ShowOptions();
                _repositoryFiles = LoadActiveRepositories();
            }
            else if (c == "3" || cl == "back" || cl == "menu" || cl == "exit" || cl == "main menu") break;
            else if (cl == "open coderunner_alpha.exe" || cl == "run coderunner_alpha.exe")
            {
                var saved = _repositoryFiles;
                _repositoryFiles = new List<string> { Og(1), Og(2), Og(3), Og(4) };
                PlayGame();
                _repositoryFiles = saved;
            }
            else { Console.Write(Red + "  access denied, operator." + Rst); Thread.Sleep(900); }
        }
        ShowCur();
    }

    private void ShowOptions()
    {
        string path = Path.Combine(RepDir, "options.txt");
        var options = new List<string>();
        try
        {
            if (File.Exists(path))
                options = File.ReadAllLines(path).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
        }
        catch { }

        while (true)
        {
            Cls(); ShowCur();
            Console.WriteLine("=== DIFFICULTY OPTIONS ===");
            Console.WriteLine($"1. Easy [{(options.Contains("easy") ? "X" : " ")}]");
            Console.WriteLine($"2. Medium [{(options.Contains("medium") ? "X" : " ")}]");
            Console.WriteLine($"3. Hard [{(options.Contains("hard") ? "X" : " ")}]");
            Console.WriteLine($"4. Very Hard [{(options.Contains("veryhard") ? "X" : " ")}]");
            Console.Write("0. Back\n>> ");

            string c = Console.ReadLine() ?? "";
            if (c == "0") break;

            string? diff = c switch { "1" => "easy", "2" => "medium", "3" => "hard", "4" => "veryhard", _ => null };
            if (diff == null) continue;

            if (options.Contains(diff)) options.Remove(diff);
            else options.Add(diff);

            try { Directory.CreateDirectory(RepDir); File.WriteAllLines(path, options); }
            catch { }
        }
    }

    private static void TryOpenWebcam()
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd",
                Arguments = "/c start ms-settings:privacy-webcam",
                CreateNoWindow = true,
                UseShellExecute = false
            });
        }
        catch { }
    }

    private const int VkF12 = 0x7B;

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);
}
