using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static Cara.Ui.ConsoleUi;

namespace Cara.Fx;

/// Hacker-space boot: spawns two extra console windows (code wall + node scan)
/// beside the main loader window, then cleans them up. The whole effect is
/// cosmetic and degrades to a plain single-window boot if windows can't spawn.
public static class BootPanes
{
    // Main entry: fire up the side panes, run the loader here, tear panes down.
    public static void LaunchHackerBoot()
    {
        var kids = new List<Process>();
        try
        {
            string? exe = Environment.ProcessPath;
            bool canSpawn = exe != null
                && !exe.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase)
                && !exe.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase);
            if (canSpawn)
            {
                Spawn(kids, exe!, "--pane code 0");
                Spawn(kids, exe!, "--pane scan 2");
                try { PositionSelf(1); } catch { }
            }
        }
        catch { }

        Cara.Core.Loader.ShowLoading();

        foreach (var p in kids)
            try { if (!p.HasExited) p.Kill(); } catch { }
        try { RestoreSelf(); } catch { }
    }

    private static void Spawn(List<Process> kids, string exe, string args)
    {
        try
        {
            var p = Process.Start(new ProcessStartInfo(exe, args) { UseShellExecute = true });
            if (p != null) kids.Add(p);
        }
        catch { }
    }

    // Entry for a spawned pane process (called from Program when args start --pane).
    public static void RunPane(string kind, int col)
    {
        try { PositionSelf(col); } catch { }
        HideCur();
        DateTime deadline = DateTime.Now.AddSeconds(10);
        try
        {
            if (kind == "code") CodeWall(deadline);
            else ScanAnim(deadline);
        }
        catch { }
        ShowCur();
    }

    private static void CodeWall(DateTime deadline)
    {
        var rng = new Random();
        string[] toks =
        {
            "mov", "jmp", "push", "pop", "call", "ret", "nop", "xor", "and", "shl",
            "shr", "rax", "rbx", "rsi", "rdi", "0x1A2B", "0xDEADBEEF", "0xFF", "ptr",
            "while(1)", "if(r)", "malloc(", "free()", "sys_read", "sys_write", "fork()",
            "exec(", "0x80", "seg++", "loop", "{", "}", "||", "&&", "==0", "!=0",
        };
        int w = SafeWidth();
        Console.Write(Grn + "\x1b[2J\x1b[H");
        while (DateTime.Now < deadline)
        {
            var sb = new StringBuilder();
            int len = 0;
            while (len < w - 6)
            {
                string t = toks[rng.Next(toks.Length)] + " ";
                sb.Append(t);
                len += t.Length;
            }
            sb.Append('\n');
            Console.Write(sb.ToString());
            Thread.Sleep(18);
        }
        Console.Write(Rst);
    }

    private static void ScanAnim(DateTime deadline)
    {
        var rng = new Random();
        DateTime start = DateTime.Now;
        double totalMs = (deadline - start).TotalMilliseconds;
        Console.Write("\x1b[2J\x1b[H");
        Console.Write(Cyan + "  DECRYPTION / NODE SCAN\n\n" + Rst);
        int line = 0;
        while (DateTime.Now < deadline)
        {
            var sb = new StringBuilder();
            sb.Append(Gry).Append("  ").Append((line * 16).ToString("X6")).Append("  ").Append(Grn2);
            for (int b = 0; b < 12; b++) sb.Append(rng.Next(256).ToString("X2")).Append(' ');
            sb.Append(Rst).Append('\n');
            Console.Write(sb.ToString());
            line++;
            Thread.Sleep(40);
            if (line % 6 == 0)
            {
                double frac = (DateTime.Now - start).TotalMilliseconds / totalMs;
                if (frac > 1) frac = 1;
                int fill = (int)(frac * 30);
                Console.Write(Cyan + "  [" + new string('#', fill) + new string('-', 30 - fill)
                    + "] " + (int)(frac * 100) + "%\n" + Rst);
            }
        }
    }

    private static int SafeWidth()
    {
        try { int w = Console.WindowWidth; return w > 10 ? w : 80; }
        catch { return 80; }
    }

    private static void PositionSelf(int col)
    {
        IntPtr hwnd = GetConsoleWindow();
        if (hwnd == IntPtr.Zero) return;
        int sw = GetSystemMetrics(0), sh = GetSystemMetrics(1);
        if (sw <= 0 || sh <= 0) return;
        int colW = sw / 3;
        MoveWindow(hwnd, col * colW, 0, colW, sh, true);
    }

    private static void RestoreSelf()
    {
        IntPtr hwnd = GetConsoleWindow();
        if (hwnd == IntPtr.Zero) return;
        int sw = GetSystemMetrics(0), sh = GetSystemMetrics(1);
        if (sw <= 0 || sh <= 0) return;
        MoveWindow(hwnd, sw / 8, 0, sw * 3 / 4, sh, true);
        SetForegroundWindow(hwnd);
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);
}
