using System.Runtime.InteropServices;
using System.Text;

namespace Cara.Ui;

/// Shared console helpers: ANSI colors, screen control and framed-box drawing,
/// so every minigame renders in the same style.
public static class ConsoleUi
{
    public const string Rst = "\x1b[0m";
    public const string Bold = "\x1b[1m";
    public const string Grn = "\x1b[38;5;46m";
    public const string Grn2 = "\x1b[38;5;34m";
    public const string Cyan = "\x1b[38;5;51m";
    public const string Yel = "\x1b[38;5;226m";
    public const string Red = "\x1b[38;5;196m";
    public const string Gry = "\x1b[38;5;240m";

    private const int Inw = 54;

    public static void Cls() => Console.Write("\x1b[2J\x1b[3J\x1b[H");
    public static void HideCur() => Console.Write("\x1b[?25l");
    public static void ShowCur() => Console.Write("\x1b[?25h");

    public static string Hline(string l, string r)
    {
        var sb = new StringBuilder("  ");
        sb.Append(l);
        for (int i = 0; i < Inw; i++) sb.Append('═');
        sb.Append(r);
        return sb.ToString();
    }

    public static string Row(string content)
    {
        if (content.Length < Inw) content = content.PadRight(Inw);
        else if (content.Length > Inw) content = content[..Inw];
        return "  ║" + content + "║";
    }

    public static string Gauge(int v, int mx, string col)
    {
        if (v < 0) v = 0;
        const int w = 22;
        int fill = mx > 0 ? v * w / mx : 0;
        if (fill > w) fill = w;
        var sb = new StringBuilder(col);
        for (int k = 0; k < w; k++) sb.Append(k < fill ? '█' : '░');
        sb.Append(Rst);
        return sb.ToString();
    }

    public static int ReadInt(int fallback)
    {
        string? s = Console.ReadLine();
        return int.TryParse(s, out int v) ? v : fallback;
    }

    public static char ReadKey()
    {
        string? s = Console.ReadLine();
        return string.IsNullOrEmpty(s) ? ' ' : char.ToLowerInvariant(s[0]);
    }

    public static void Pause(string msg)
    {
        Console.Write(Grn + "\n  " + msg + Rst);
        ShowCur();
        Console.ReadLine();
    }

    public static void EnableVt()
    {
        IntPtr handle = GetStdHandle(-11);
        if (GetConsoleMode(handle, out uint mode))
            SetConsoleMode(handle, mode | 0x0004);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);
}
