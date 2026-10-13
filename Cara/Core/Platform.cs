using System.Diagnostics;
using System.Runtime.Versioning;

namespace Cara.Core;

/// Cross-platform helpers: route system actions to Windows or Linux equivalents.
public static class Platform
{
    [SupportedOSPlatformGuard("windows")]
    public static bool IsWindows => OperatingSystem.IsWindows();

    [SupportedOSPlatformGuard("linux")]
    public static bool IsLinux => OperatingSystem.IsLinux();

    /// Run a shell command through cmd.exe (Windows) or /bin/bash (Linux), wait, return exit code.
    public static int Run(string command)
    {
        try
        {
            var psi = IsWindows
                ? new ProcessStartInfo("cmd.exe", "/c " + command) { UseShellExecute = false }
                : new ProcessStartInfo("/bin/bash", "-c \"" + Esc(command) + "\"") { UseShellExecute = false };
            using var p = Process.Start(psi);
            if (p == null) return -1;
            p.WaitForExit();
            return p.ExitCode;
        }
        catch { return -1; }
    }

    /// Fire a shell command and do not wait.
    public static void RunDetached(string command)
    {
        try
        {
            var psi = IsWindows
                ? new ProcessStartInfo("cmd.exe", "/c " + command) { UseShellExecute = false, CreateNoWindow = true }
                : new ProcessStartInfo("/bin/bash", "-c \"" + Esc(command) + "\"") { UseShellExecute = false };
            Process.Start(psi);
        }
        catch { }
    }

    /// Start a program directly (no shell). Returns the process or null.
    public static Process? Start(string file, string args, bool newWindow = false)
    {
        try { return Process.Start(new ProcessStartInfo(file, args) { UseShellExecute = newWindow, CreateNoWindow = !newWindow }); }
        catch { return null; }
    }

    public static void OpenUrl(string url)
    {
        if (IsWindows) RunDetached("start \"\" \"" + url + "\"");
        else if (Start("xdg-open", url) == null) RunDetached("xdg-open '" + url + "' >/dev/null 2>&1 &");
    }

    public static void Beep(int freq, int dur)
    {
        try
        {
            if (OperatingSystem.IsWindows()) Console.Beep(freq, dur);
            else { Console.Out.Write('\a'); Console.Out.Flush(); Thread.Sleep(Math.Min(dur, 120)); }
        }
        catch { }
    }

    public static void Notify(string title, string text)
    {
        if (IsWindows) Console.WriteLine("[" + title + "] " + text);
        else if (Start("notify-send", "\"" + title + "\" \"" + text + "\"") == null) Console.WriteLine("[" + title + "] " + text);
    }

    public static void Shutdown(int seconds)
    {
        if (IsWindows) Run("shutdown /s /t " + seconds);
        else RunDetached("shutdown -h +" + Math.Max(1, (seconds + 59) / 60) + " 2>/dev/null || systemctl poweroff 2>/dev/null");
    }

    public static void CancelShutdown()
    {
        if (IsWindows) Run("shutdown /a");
        else RunDetached("shutdown -c 2>/dev/null");
    }

    // ---- radios / network (Linux = rfkill / nmcli; Windows handled in SystemFx) ----
    public static void WifiOff() { if (!IsWindows) Run("rfkill block wifi 2>/dev/null"); }
    public static void RadioOff() { if (!IsWindows) Run("rfkill block all 2>/dev/null"); }
    public static void RadioOn() { if (!IsWindows) Run("rfkill unblock all 2>/dev/null; nmcli networking on 2>/dev/null; nmcli radio all on 2>/dev/null"); }
    public static void BluetoothOn() { if (!IsWindows) Run("rfkill unblock bluetooth 2>/dev/null; systemctl start bluetooth 2>/dev/null"); }
    public static void NetworkRecover()
    {
        if (!IsWindows)
            Run("rfkill unblock all 2>/dev/null; nmcli networking on 2>/dev/null; nmcli radio all on 2>/dev/null; " +
                "systemctl restart NetworkManager 2>/dev/null; systemctl start bluetooth 2>/dev/null");
    }

    // ---- creepy audio (Linux players; Windows uses winmm in SystemFx) ----
    private static Process? _audio;
    public static void PlayAudioLoop(string file)
    {
        StopAudio();
        if (IsWindows) return;
        foreach (var (bin, args) in new[]
        {
            ("mpg123", "--loop -1 \"" + file + "\""),
            ("ffplay", "-nodisp -autoexit -loop 0 \"" + file + "\""),
            ("paplay", "\"" + file + "\""),
        })
        {
            var p = Start(bin, args);
            if (p != null) { _audio = p; return; }
        }
    }
    public static void StopAudio() { try { if (_audio != null && !_audio.HasExited) _audio.Kill(); } catch { } _audio = null; }

    // ---- spawn a terminal window running a shell script (Linux satellite panes / haunt) ----
    private static readonly string[] Terminals =
    {
        "x-terminal-emulator", "gnome-terminal", "konsole", "xfce4-terminal", "xterm", "kitty", "alacritty",
    };

    public static Process? SpawnTerminal(string title, string bashCommand)
    {
        if (IsWindows) return null;
        foreach (var term in Terminals)
        {
            string args = term switch
            {
                "gnome-terminal" => "--title=\"" + title + "\" -- bash -c \"" + Esc(bashCommand) + "\"",
                "konsole" => "-p tabtitle=\"" + title + "\" -e bash -c \"" + Esc(bashCommand) + "\"",
                "xfce4-terminal" => "--title=\"" + title + "\" -e 'bash -c \"" + Esc(bashCommand) + "\"'",
                "kitty" => "--title \"" + title + "\" bash -c \"" + Esc(bashCommand) + "\"",
                "alacritty" => "-t \"" + title + "\" -e bash -c \"" + Esc(bashCommand) + "\"",
                _ => "-T \"" + title + "\" -e bash -c \"" + Esc(bashCommand) + "\"",   // xterm / x-terminal-emulator
            };
            var p = Start(term, args, newWindow: false);
            if (p != null) return p;
        }
        return null;
    }

    /// X11 screen size via xdotool, else a 1080p default. Best-effort.
    public static (int W, int H) ScreenSize()
    {
        if (!IsLinux) return (1920, 1080);
        try
        {
            var psi = new ProcessStartInfo("xdotool", "getdisplaygeometry") { RedirectStandardOutput = true, UseShellExecute = false };
            using var p = Process.Start(psi);
            if (p != null)
            {
                string o = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                var parts = o.Trim().Split(' ');
                if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h) && w > 0 && h > 0)
                    return (w, h);
            }
        }
        catch { }
        return (1920, 1080);
    }

    /// Move/size an X11 window by title via wmctrl or xdotool, if available. Best-effort.
    public static void MoveWindowByTitle(string title, int x, int y, int w, int h)
    {
        if (IsWindows) return;
        RunDetached("wmctrl -r \"" + title + "\" -e 0," + x + "," + y + "," + w + "," + h +
                    " 2>/dev/null || (xdotool search --name \"" + title + "\" windowmove " + x + " " + y +
                    " windowsize %1 " + w + " " + h + " 2>/dev/null)");
    }

    private static string Esc(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
}
