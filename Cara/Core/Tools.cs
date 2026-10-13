using System.Data;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using static Cara.Ui.ConsoleUi;

namespace Cara.Core;

/// Normal, useful offline commands: calc, notes, timer, sysinfo, genpass,
/// world clock and base conversion.
public static class Tools
{
    private static readonly string NotesPath =
        Path.Combine(AppContext.BaseDirectory, "resources", "data", "notes.txt");

    public static void Calc(string expr)
    {
        expr = expr.Trim();
        if (expr.Length == 0) { Console.WriteLine(Gry + "  usage: calc 2+2*3" + Rst); return; }
        try
        {
            object r = new DataTable().Compute(expr, null);
            Console.WriteLine(Grn + "  = " + Convert.ToString(r, CultureInfo.InvariantCulture) + Rst);
        }
        catch { Console.WriteLine(Red + "  invalid expression" + Rst); }
    }

    public static void Notes(string args)
    {
        var parts = args.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        string cmd = parts.Length > 0 ? parts[0].ToLowerInvariant() : "list";
        string rest = parts.Length > 1 ? parts[1] : "";

        switch (cmd)
        {
            case "add":
                if (rest.Length == 0) { Console.WriteLine(Gry + "  usage: note add <text>" + Rst); return; }
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(NotesPath)!);
                    File.AppendAllText(NotesPath, rest + Environment.NewLine);
                    Console.WriteLine(Grn + "  noted." + Rst);
                }
                catch { Console.WriteLine(Red + "  could not save note" + Rst); }
                break;

            case "remove":
            case "rm":
                if (int.TryParse(rest.Trim(), out int n)) RemoveNote(n);
                else Console.WriteLine(Gry + "  usage: note remove <index>" + Rst);
                break;

            case "clear":
                try { if (File.Exists(NotesPath)) File.Delete(NotesPath); } catch { }
                Console.WriteLine(Grn + "  notes cleared." + Rst);
                break;

            case "list":
            default:
                ListNotes();
                break;
        }
    }

    private static void ListNotes()
    {
        var lines = ReadNotes();
        if (lines.Count == 0) { Console.WriteLine(Gry + "  (no notes - 'note add <text>')" + Rst); return; }
        Console.WriteLine(Grn + "  NOTES" + Rst);
        for (int i = 0; i < lines.Count; i++)
            Console.WriteLine(Grn2 + "  " + (i + 1) + ". " + Rst + lines[i]);
    }

    private static void RemoveNote(int index)
    {
        var lines = ReadNotes();
        if (index < 1 || index > lines.Count) { Console.WriteLine(Red + "  invalid index" + Rst); return; }
        lines.RemoveAt(index - 1);
        try { File.WriteAllLines(NotesPath, lines); Console.WriteLine(Grn + "  removed." + Rst); }
        catch { Console.WriteLine(Red + "  could not update notes" + Rst); }
    }

    private static List<string> ReadNotes()
    {
        try
        {
            if (File.Exists(NotesPath))
                return File.ReadAllLines(NotesPath).Where(l => l.Trim().Length > 0).ToList();
        }
        catch { }
        return new List<string>();
    }

    public static void Timer(string arg)
    {
        int secs = ParseDuration(arg);
        if (secs <= 0) { Console.WriteLine(Gry + "  usage: timer 90s | 5m | 1h30m | MM:SS" + Rst); return; }

        HideCur();
        DateTime end = DateTime.Now.AddSeconds(secs);
        while (true)
        {
            int rem = (int)Math.Ceiling((end - DateTime.Now).TotalSeconds);
            if (rem < 0) rem = 0;
            double frac = (double)rem / secs;
            string col = frac > 0.5 ? Grn : frac > 0.25 ? Yel : Red;
            Console.Write("\r  " + col + "TIMER " + (rem / 3600).ToString("00") + ":"
                + ((rem % 3600) / 60).ToString("00") + ":" + (rem % 60).ToString("00")
                + Rst + Gry + "   (any key to cancel)   " + Rst);
            if (rem <= 0) break;

            DateTime t0 = DateTime.Now;
            while ((DateTime.Now - t0).TotalMilliseconds < 250)
            {
                if (!Console.IsInputRedirected && Console.KeyAvailable)
                {
                    Console.ReadKey(true);
                    Console.Write("\n" + Yel + "  timer cancelled\n" + Rst);
                    ShowCur();
                    return;
                }
                Thread.Sleep(20);
            }
        }
        ShowCur();
        Console.Write("\n" + Grn + Bold + "  >> TIME'S UP <<\n" + Rst);
        for (int i = 0; i < 3; i++) { Console.Write("\a"); Thread.Sleep(250); }
    }

    private static int ParseDuration(string s)
    {
        s = s.Trim().ToLowerInvariant();
        if (s.Length == 0) return -1;

        if (s.Contains(':'))
        {
            var p = s.Split(':');
            if (p.Length == 2 && int.TryParse(p[0], out int m2) && int.TryParse(p[1], out int s2))
                return m2 * 60 + s2;
            if (p.Length == 3 && int.TryParse(p[0], out int h3) && int.TryParse(p[1], out int m3) && int.TryParse(p[2], out int s3))
                return h3 * 3600 + m3 * 60 + s3;
            return -1;
        }

        int total = 0, num = 0;
        bool anyDigit = false;
        foreach (char c in s)
        {
            if (char.IsDigit(c)) { num = num * 10 + (c - '0'); anyDigit = true; }
            else if (c == 'h') { total += num * 3600; num = 0; }
            else if (c == 'm') { total += num * 60; num = 0; }
            else if (c == 's') { total += num; num = 0; }
            else if (c == ' ') { }
            else return -1;
        }
        total += num;   // leftover digits (bare number or trailing unitless) = seconds
        return anyDigit ? total : -1;
    }

    public static void GenPass(string arg)
    {
        int len = 16;
        if (int.TryParse(arg.Trim(), out int n)) len = n;
        if (len < 4) len = 4;
        if (len > 128) len = 128;

        const string chars = "abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789!@#$%^&*-_=+?";
        var sb = new StringBuilder();
        for (int i = 0; i < len; i++) sb.Append(chars[RandomNumberGenerator.GetInt32(chars.Length)]);
        Console.WriteLine(Grn + "  " + sb + Rst);
    }

    public static void BaseConv(string arg)
    {
        arg = arg.Trim();
        long val;
        try
        {
            if (arg.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) val = Convert.ToInt64(arg[2..], 16);
            else if (arg.StartsWith("0b", StringComparison.OrdinalIgnoreCase)) val = Convert.ToInt64(arg[2..], 2);
            else if (arg.StartsWith("0o", StringComparison.OrdinalIgnoreCase)) val = Convert.ToInt64(arg[2..], 8);
            else val = long.Parse(arg, CultureInfo.InvariantCulture);
        }
        catch { Console.WriteLine(Red + "  invalid number (try 255, 0xFF, 0b1010)" + Rst); return; }

        Console.WriteLine(Grn + "  DEC  " + val + Rst);
        Console.WriteLine(Grn + "  HEX  0x" + Convert.ToString(val, 16).ToUpperInvariant() + Rst);
        Console.WriteLine(Grn + "  OCT  0o" + Convert.ToString(val, 8) + Rst);
        Console.WriteLine(Grn + "  BIN  0b" + Convert.ToString(val, 2) + Rst);
    }

    public static void SysInfo()
    {
        long upSec = Environment.TickCount64 / 1000;
        string uptime = (upSec / 86400) + "d " + ((upSec % 86400) / 3600) + "h " + ((upSec % 3600) / 60) + "m";
        long mem = 0;
        try { mem = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64; } catch { }

        Console.WriteLine(Grn + "  SYSTEM INFO" + Rst);
        Console.WriteLine(Gry + "  " + new string('-', 44) + Rst);
        Line("OS", RuntimeInformation.OSDescription);
        Line("Arch", RuntimeInformation.OSArchitecture.ToString());
        Line("Machine", Environment.MachineName);
        Line("User", Environment.UserName);
        Line("CPUs", Environment.ProcessorCount.ToString());
        Line("64-bit OS", Environment.Is64BitOperatingSystem.ToString());
        Line("Runtime", RuntimeInformation.FrameworkDescription);
        Line("Uptime", uptime);
        Line("App mem", (mem / 1048576) + " MB");
        try
        {
            foreach (var d in DriveInfo.GetDrives())
                if (d.IsReady)
                    Line("Drive " + d.Name.TrimEnd('\\'),
                        (d.AvailableFreeSpace / 1073741824) + " GB free / " + (d.TotalSize / 1073741824) + " GB");
        }
        catch { }
    }

    private static void Line(string k, string v)
        => Console.WriteLine(Grn2 + "  " + k.PadRight(12) + Rst + ": " + v);

    public static void Clock()
    {
        var zones = new List<(string Label, TimeZoneInfo Tz)>();
        void Try(string label, string id) { try { zones.Add((label, TimeZoneInfo.FindSystemTimeZoneById(id))); } catch { } }
        Try("New York", "Eastern Standard Time");
        Try("London", "GMT Standard Time");
        Try("Tokyo", "Tokyo Standard Time");

        HideCur();
        Console.Write("\x1b[2J");
        while (true)
        {
            if (!Console.IsInputRedirected && Console.KeyAvailable) { Console.ReadKey(true); break; }
            DateTime now = DateTime.Now;
            var sb = new StringBuilder("\x1b[H");
            sb.Append(Grn + Bold + "  WORLD CLOCK" + Rst + Gry + "   (any key to exit)" + Rst + "\x1b[K\n\n");
            sb.Append(ClockRow("LOCAL", now));
            sb.Append(ClockRow("UTC", now.ToUniversalTime()));
            foreach (var z in zones)
                sb.Append(ClockRow(z.Label, TimeZoneInfo.ConvertTime(now, z.Tz)));
            sb.Append("\x1b[J");
            Console.Write(sb.ToString());
            Thread.Sleep(200);
            if (Console.IsInputRedirected) break;   // no console to wait on
        }
        ShowCur();
    }

    private static string ClockRow(string label, DateTime t)
        => "  " + Grn2 + label.PadRight(8) + Rst + "  " + Cyan + t.ToString("HH:mm:ss")
           + Rst + "  " + Gry + t.ToString("ddd dd MMM", CultureInfo.InvariantCulture) + Rst + "\x1b[K\n";
}
