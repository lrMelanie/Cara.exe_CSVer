using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
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

    private static readonly string[] DiffKeys = { "easy", "medium", "hard", "veryhard" };
    private static readonly string[] DiffNames = { "EASY", "MEDIUM", "HARD", "V.HARD" };
    private static readonly string[] LangKeys = { "csharp", "js", "python", "cpp", "c", "ruby", "lua", "bat", "asm", "perl", "powershell" };
    private static readonly string[] LangNames = { "C#", "JS", "PY", "C++", "C", "Ruby", "Lua", "BAT", "ASM", "Perl", "PS" };
    private static readonly string[] FunLangKeys = { "brainfuck", "lolcode", "ook", "malbolge" };
    private static readonly string[] FunLangNames = { "brainfuck", "lolcode", "ook", "malbolge" };

    private static List<string> LoadActiveRepositories()
    {
        var active = new List<string>();
        var sel = LoadSelection();
        for (int d = 0; d < DiffKeys.Length; d++)
            for (int l = 0; l < LangKeys.Length; l++)
                if (sel[d, l])
                    AddCellFiles(active, d, l);

        var funSel = LoadFunSelection();
        for (int j = 0; j < FunLangKeys.Length; j++)
            if (funSel[j])
                AddFunCellFiles(active, j);

        if (LoadOldFiles())
            for (int n = 1; n <= 4; n++)
            {
                string f = Og(n);
                if (File.Exists(f)) active.Add(f);
            }

        if (active.Count == 0)
            for (int d = 0; d < DiffKeys.Length; d++)
                for (int l = 0; l < LangKeys.Length; l++)
                    AddCellFiles(active, d, l);

        if (active.Count == 0)
        {
            if (File.Exists(Rep("repository1.txt"))) active.Add(Rep("repository1.txt"));
            if (File.Exists(Rep("repository5.txt"))) active.Add(Rep("repository5.txt"));
        }
        return active;
    }

    private static void AddCellFiles(List<string> active, int d, int l)
    {
        string baseName = $"{DiffKeys[d]}_{LangKeys[l]}";
        string f0 = Rep(baseName + ".txt");
        if (File.Exists(f0)) active.Add(f0);
        for (int n = 2; n <= 5; n++)
        {
            string fn = Rep(baseName + "_" + n + ".txt");
            if (File.Exists(fn)) active.Add(fn);
        }
    }

    private static void AddFunCellFiles(List<string> active, int j)
    {
        string baseName = $"impossible_{FunLangKeys[j]}";
        string f0 = Rep(baseName + ".txt");
        if (File.Exists(f0)) active.Add(f0);
        for (int n = 2; n <= 5; n++)
        {
            string fn = Rep(baseName + "_" + n + ".txt");
            if (File.Exists(fn)) active.Add(fn);
        }
    }

    private static bool[,] LoadSelection()
    {
        var sel = new bool[DiffKeys.Length, LangKeys.Length];
        try
        {
            string path = Path.Combine(RepDir, "options.txt");
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Trim().Split(':');
                    if (parts.Length != 2) continue;
                    int d = Array.IndexOf(DiffKeys, parts[0]);
                    int l = Array.IndexOf(LangKeys, parts[1]);
                    if (d >= 0 && l >= 0) sel[d, l] = true;
                }
        }
        catch { }
        return sel;
    }

    private static void SaveSelection(bool[,] sel, bool[] funSel, bool oldFiles)
    {
        var lines = new List<string>();
        for (int d = 0; d < DiffKeys.Length; d++)
            for (int l = 0; l < LangKeys.Length; l++)
                if (sel[d, l]) lines.Add(DiffKeys[d] + ":" + LangKeys[l]);
        for (int j = 0; j < FunLangKeys.Length; j++)
            if (funSel[j]) lines.Add("fun:" + FunLangKeys[j]);
        if (oldFiles) lines.Add("oldfiles:on");
        try { Directory.CreateDirectory(RepDir); File.WriteAllLines(Path.Combine(RepDir, "options.txt"), lines); }
        catch { }
    }

    private static bool[] LoadFunSelection()
    {
        var funSel = new bool[FunLangKeys.Length];
        try
        {
            string path = Path.Combine(RepDir, "options.txt");
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                {
                    var parts = line.Trim().Split(':');
                    if (parts.Length != 2 || parts[0] != "fun") continue;
                    int j = Array.IndexOf(FunLangKeys, parts[1]);
                    if (j >= 0) funSel[j] = true;
                }
        }
        catch { }
        return funSel;
    }

    private static bool LoadOldFiles()
    {
        try
        {
            string path = Path.Combine(RepDir, "options.txt");
            if (File.Exists(path))
                foreach (var line in File.ReadAllLines(path))
                    if (line.Trim() == "oldfiles:on") return true;
        }
        catch { }
        return false;
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

    // Redraws the whole frame from the top (no full clear -> no flicker) and
    // shows the typed buffer coloured per character: green where it matches the
    // target, red on the first typo (real-time, WTTG style).
    private void RenderTyping(int rem, int max, List<string> lines, int cur, string typed, string target)
    {
        var o = new StringBuilder();
        o.Append("\x1b[H");
        string info = "  PTS " + _points + "    LVL " + _linesToType + "    BEST " + _highScore;
        o.Append(Grn).Append(Hline("╔", "╗")).Append("\x1b[K\n");
        o.Append(Row("  CARA://CODE_RUNNER              [ UPLINK ACTIVE ]")).Append("\x1b[K\n");
        o.Append(Hline("╠", "╣")).Append("\x1b[K\n");
        o.Append(Row(info)).Append("\x1b[K\n");
        o.Append(Hline("╚", "╝")).Append(Rst).Append("\x1b[K\n");
        o.Append("\x1b[K\n");
        o.Append("   TRACE ").Append(TimeBar(rem, max)).Append("  ").Append(Grn).Append(rem).Append('s').Append(Rst).Append("\x1b[K\n");
        o.Append("\x1b[K\n");
        for (int j = 0; j < lines.Count; j++)
        {
            if (j < cur) o.Append(Grn2).Append("   [OK] ").Append(lines[j]).Append(Rst);
            else if (j == cur) o.Append(Bold).Append(Cyan).Append("   >>>  ").Append(lines[j]).Append(Rst);
            else o.Append(Gry).Append("    ..  ").Append(lines[j]).Append(Rst);
            o.Append("\x1b[K\n");
        }
        o.Append("\x1b[K\n");
        o.Append(Grn).Append("  root@cara:~# ").Append(Rst);
        for (int i = 0; i < typed.Length; i++)
        {
            bool ok = i < target.Length && typed[i] == target[i];
            o.Append(ok ? Grn2 : Red).Append(typed[i]);
        }
        o.Append(Rst).Append("\x1b[K\x1b[J");
        Console.Write(o.ToString());
        ShowCur();
    }

    // Reads the current line character by character with live colouring and a
    // live time trace. Returns (timedOut, typed); timedOut ends the run.
    private (bool timedOut, string typed) TypeLine(List<string> lines, int cur, DateTime endTime, int maxS)
    {
        var sb = new StringBuilder();
        string target = lines[cur];
        Cls();
        while (true)
        {
            if (DateTime.Now >= endTime) return (true, sb.ToString());
            int rem = (int)(endTime - DateTime.Now).TotalSeconds;
            if (rem < 0) rem = 0;
            RenderTyping(rem, maxS, lines, cur, sb.ToString(), target);

            DateTime refresh = DateTime.Now;
            while (!Console.KeyAvailable)
            {
                if (DateTime.Now >= endTime) return (true, sb.ToString());
                Thread.Sleep(30);
                if ((DateTime.Now - refresh).TotalMilliseconds > 250) break;
            }
            if (!Console.KeyAvailable) continue;

            ConsoleKeyInfo key = Console.ReadKey(intercept: true);
            if (key.Key == ConsoleKey.Enter) return (false, sb.ToString());
            if (key.Key == ConsoleKey.Backspace) { if (sb.Length > 0) sb.Length--; continue; }
            if (char.IsControl(key.KeyChar)) continue;
            sb.Append(key.KeyChar);
        }
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

        var sel = LoadSelection();
        var counts = new StringBuilder("   repos enabled   ");
        for (int d = 0; d < DiffKeys.Length; d++)
        {
            int n = 0;
            for (int l = 0; l < LangKeys.Length; l++) if (sel[d, l]) n++;
            counts.Append(DiffNames[d]).Append(':').Append(n).Append("   ");
        }
        var funSel = LoadFunSelection();
        int fn = 0;
        for (int j = 0; j < FunLangKeys.Length; j++) if (funSel[j]) fn++;
        if (fn > 0) counts.Append("FUN:").Append(fn).Append("   ");
        if (LoadOldFiles()) counts.Append("OLD:4");
        Console.Write(Rst + Gry + "\n" + counts + "\n");

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
                Cara.Fx.Haunt.Maybe(250);
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
                    var (timedOut, input) = TypeLine(current, i, endTime, MaxS);
                    if (timedOut) { SaveHighScore(); goto gameOver; }

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
        var sel = LoadSelection();
        var funSel = LoadFunSelection();
        bool oldFiles = LoadOldFiles();
        bool unlocked = oldFiles;   // already-on means already discovered
        string codeBuf = "";
        int fr = 0, fc = 0;
        int funRow = DiffKeys.Length + 1;
        int oldRow = DiffKeys.Length + 2;

        while (true)
        {
            RenderMatrix(sel, funSel, oldFiles, unlocked, fr, fc);
            ConsoleKeyInfo k = Console.ReadKey(intercept: true);
            if (k.Key == ConsoleKey.Escape) break;

            // secret: typing 2025 unlocks the Old Files category
            if (!unlocked && char.IsDigit(k.KeyChar))
            {
                codeBuf += k.KeyChar;
                if (codeBuf.Length > 8) codeBuf = codeBuf.Substring(codeBuf.Length - 8);
                if (codeBuf.EndsWith("2025")) unlocked = true;
                continue;
            }

            switch (k.Key)
            {
                case ConsoleKey.UpArrow: fr = Math.Max(0, fr - 1); break;
                case ConsoleKey.DownArrow: fr = Math.Min(oldRow, fr + 1); break;
                case ConsoleKey.LeftArrow: fc = Math.Max(0, fc - 1); break;
                case ConsoleKey.RightArrow: fc++; break;
                case ConsoleKey.Enter:
                case ConsoleKey.Spacebar:
                    if (fr == oldRow)
                    {
                        if (unlocked) { oldFiles = !oldFiles; SaveSelection(sel, funSel, oldFiles); }
                    }
                    else if (fr == funRow) { ToggleFun(funSel, fc); SaveSelection(sel, funSel, oldFiles); }
                    else { ToggleFocus(sel, fr, fc); SaveSelection(sel, funSel, oldFiles); }
                    break;
            }

            int maxCol;
            if (fr == 0) maxCol = LangKeys.Length - 1;
            else if (fr == funRow) maxCol = FunLangKeys.Length;
            else if (fr == oldRow) maxCol = 0;
            else maxCol = LangKeys.Length;
            if (fc > maxCol) fc = maxCol;
            if (fc < 0) fc = 0;
        }
        ShowCur();
    }

    private static void ToggleFocus(bool[,] sel, int fr, int fc)
    {
        if (fr == 0)
        {
            int l = fc;
            bool all = true;
            for (int d = 0; d < DiffKeys.Length; d++) if (!sel[d, l]) all = false;
            for (int d = 0; d < DiffKeys.Length; d++) sel[d, l] = !all;
        }
        else
        {
            int d = fr - 1;
            if (fc == 0)
            {
                bool all = true;
                for (int l = 0; l < LangKeys.Length; l++) if (!sel[d, l]) all = false;
                for (int l = 0; l < LangKeys.Length; l++) sel[d, l] = !all;
            }
            else
            {
                sel[d, fc - 1] = !sel[d, fc - 1];
            }
        }
    }

    private static void ToggleFun(bool[] funSel, int fc)
    {
        if (fc == 0)
        {
            bool all = true;
            for (int j = 0; j < funSel.Length; j++) if (!funSel[j]) all = false;
            for (int j = 0; j < funSel.Length; j++) funSel[j] = !all;
        }
        else funSel[fc - 1] = !funSel[fc - 1];
    }

    private static bool AnyFun(bool[] funSel)
    {
        for (int j = 0; j < funSel.Length; j++) if (funSel[j]) return true;
        return false;
    }

    private static bool AnyInRow(bool[,] sel, int d)
    {
        for (int l = 0; l < LangKeys.Length; l++) if (sel[d, l]) return true;
        return false;
    }

    private static bool AnyInColumn(bool[,] sel, int l)
    {
        for (int d = 0; d < DiffKeys.Length; d++) if (sel[d, l]) return true;
        return false;
    }

    private static string Tok(string label, bool active, bool focused, int width)
    {
        string name = label.Length > width ? label.Substring(0, width) : label.PadRight(width);
        string color = focused ? Cyan : (active ? Grn : Grn2);
        string open = focused ? "[" : " ";
        string close = focused ? "]" : " ";
        return color + open + name + close + Rst;
    }

    private static void RenderMatrix(bool[,] sel, bool[] funSel, bool oldFiles, bool unlocked, int fr, int fc)
    {
        Cls(); HideCur();
        Console.WriteLine(Grn + "   === CODE RUNNER :: DIFFICULTY MATRIX ===" + Rst);
        Console.WriteLine();
        Console.WriteLine(Gry + "   languages - toggle a language across ALL levels:" + Rst);
        Console.Write(new string(' ', 11));
        for (int l = 0; l < LangKeys.Length; l++)
            Console.Write(Tok(LangNames[l], AnyInColumn(sel, l), fr == 0 && fc == l, 4));
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine(Gry + "   levels - the name toggles the whole row:" + Rst);
        for (int d = 0; d < DiffKeys.Length; d++)
        {
            Console.Write("   " + Tok(DiffNames[d], AnyInRow(sel, d), fr == d + 1 && fc == 0, 6));
            for (int l = 0; l < LangKeys.Length; l++)
                Console.Write(Tok(LangNames[l], sel[d, l], fr == d + 1 && fc == l + 1, 4));
            Console.WriteLine();
        }
        Console.WriteLine();

        int funRow = DiffKeys.Length + 1;
        bool funNameFocus = fr == funRow && fc == 0;
        string funColor = funNameFocus ? Cyan : (AnyFun(funSel) ? Grn : Grn2);
        Console.Write("   " + funColor + (funNameFocus ? "[" : " ")
            + "\x1b[9m" + "1MP05518L3" + "\x1b[29m" + " FUN"
            + (funNameFocus ? "]" : " ") + Rst + " ");
        for (int j = 0; j < FunLangKeys.Length; j++)
            Console.Write(Tok(FunLangNames[j], funSel[j], fr == funRow && fc == j + 1, 9));
        Console.WriteLine();
        Console.WriteLine();

        int oldRow = DiffKeys.Length + 2;
        bool ofFocus = fr == oldRow && fc == 0;
        string ofColor = ofFocus ? Cyan : (oldFiles ? Grn : Grn2);
        string o = ofFocus ? "[" : " ";
        string cl = ofFocus ? "]" : " ";
        Console.Write("   " + ofColor + o + "OLD FILES" + cl + Rst);
        if (!unlocked)
            Console.Write(Gry + "   locked - something here is not obvious..." + Rst);
        else
            Console.Write((oldFiles ? Grn + "   [ ON ]  OG files active" : Grn2 + "   [ OFF ]") + Rst);
        Console.WriteLine();

        Console.WriteLine();
        Console.WriteLine(Gry + "   [arrows] move   [enter] toggle   [esc] back" + Rst);
        Console.WriteLine(Gry + "   green = active   dark green = inactive   cyan = cursor" + Rst);
        ShowCur();
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
