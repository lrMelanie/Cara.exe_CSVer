using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Cara.Core;

namespace Cara.Fx;

/// System horror/hacker routines. Windows and Linux both "really touch the machine".
public static class SystemFx
{
    private static readonly Random Rng = new();

    // Windows-only cmd command (netsh / powershell / registry). No-op on Linux.
    private static int Win(string command) => Platform.IsWindows ? Platform.Run(command) : 0;

    private static void AppendLog(string path, string line)
    {
        try
        {
            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.AppendAllText(path, line + "\n");
        }
        catch { }
    }

    private static void Color(string ansi) { try { Console.Write(ansi); } catch { } }
    private const string CReset = "\x1b[0m", CRed = "\x1b[38;5;196m", CGrn = "\x1b[38;5;46m", CWhite = "\x1b[38;5;231m";

    public static void ExecuteWelcomeSequence()
    {
        ExecuteSpectralBroadcast();
        TriggerPhantomProtocol();
        InitiateBlackMirror();

        Cara.Minigames.MinigameMenu.Run();

        Console.WriteLine("[System] All systems engaged. Game parameters initialized.");
        Platform.OpenUrl("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
    }

    public static void FixMe()
    {
        if (Platform.IsWindows)
            Win("\"resources\\data\\secure\\guardian_angel.bat\"");
        else
        {
            Platform.NetworkRecover();
            string sh = Path.Combine("resources", "data", "secure", "guardian_angel.sh");
            if (File.Exists(sh)) Platform.Run("bash \"" + sh + "\"");
        }
        Console.WriteLine("[System] network recovery attempted");
    }

    public static void EnableAirplaneMode()
    {
        const string log = "resources/logs/airplane_log.txt";
        if (Platform.IsWindows)
        {
            int regResult = Win("powershell -Command \"Try {   $regPath = 'HKLM:/SYSTEM/CurrentControlSet/Control/RadioManagement/SystemRadioState';   if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force | Out-Null };   Set-ItemProperty -Path $regPath -Name 'SystemRadioState' -Value 1 -Force -ErrorAction Stop;   Write-Output '[SUCCESS] Registry updated';   Exit 0;} Catch {   Write-Output ('[ERROR] ' + $_.Exception.Message);   Exit 1;}\"");
            AppendLog(log, "Registry update result: " + regResult);
            foreach (var iface in new[] { "Wi-Fi", "Bluetooth Network Connection", "Ethernet" })
                AppendLog(log, iface + " disable: " + Win($"netsh interface set interface \"{iface}\" admin=disable"));
            Win("powershell -Command \"Get-Service -Name 'WlanSvc', 'BthServ' | Stop-Service -Force -PassThru -ErrorAction SilentlyContinue | Set-Service -StartupType Disabled -PassThru | Out-File -Append resources/logs/airplane_log.txt\"");
        }
        else
        {
            Platform.RadioOff();
            AppendLog(log, "rfkill block all (airplane mode)");
        }
    }

    public static void ActivateBluetooth()
    {
        const string log = "resources/logs/bluetooth_log.txt";
        if (Platform.IsWindows)
        {
            const string devconPath = "resources/Tools/devcon.exe";
            string toolPath = "\"" + devconPath + "\"";
            if (!File.Exists(devconPath)) { AppendLog(log, "ERROR: devcon.exe not found at: " + toolPath); return; }
            var commands = new[]
            {
                toolPath + " enable *DEV_0A12*",
                toolPath + " enable *USB\\VID_0A12*",
                toolPath + " rescan",
                "powershell -Command \"Start-Service -Name 'BthServ' -ErrorAction SilentlyContinue;Start-Service -Name 'BTAGService' -ErrorAction SilentlyContinue;Start-Sleep -Seconds 3;Add-BluetoothDevice -Name '*' -ErrorAction SilentlyContinue | Out-File -Append " + log + "\"",
            };
            foreach (var cmd in commands) { AppendLog(log, "Executing: " + cmd); AppendLog(log, "Result: " + Win(cmd)); }
            Win("netsh interface set interface \"Bluetooth Network Connection\" admin=enable");
        }
        else
        {
            Platform.BluetoothOn();
            AppendLog(log, "rfkill unblock bluetooth + start bluetooth.service");
        }
    }

    private static void PlayCreepyAudio()
    {
        string soundPath = Path.Combine("resources", "audio", "c1337.mp3");
        if (File.Exists(soundPath))
        {
            if (Platform.IsWindows) PlayMp3Windows(soundPath);
            else Platform.PlayAudioLoop(soundPath);
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                Platform.Beep(300 + i * 100, 200); Color(CRed);
                Platform.Beep(1500 - i * 100, 200); Color(CReset);
            }
        }
    }

    public static void ExecuteSpectralBroadcast()
    {
        for (int i = 0; i < 5; i++)
        {
            Color(CGrn); Thread.Sleep(50);
            Color(CRed); Thread.Sleep(50);
        }
        Color(CReset);

        if (Platform.IsWindows)
        {
            Win("netsh interface set interface \"Wi-Fi\" admin=disable");
            Platform.OpenUrl("microsoft.windows.camera:");
        }
        else
        {
            Platform.WifiOff();
            if (Platform.Start("cheese", "") == null) Platform.Start("guvcview", "");
        }
        Console.WriteLine("\n[!] Camera feed accessed");
        PlayCreepyAudio();
        if (Platform.IsWindows)
            Win("powershell -Command \"(New-Object Media.SoundPlayer 'C:/Windows/Media/Windows Background.wav').PlaySync();\"");

        try
        {
            string loc = Path.Combine(Path.GetTempPath(), "location.log");
            File.WriteAllText(loc,
                "Last known coordinates:\n" +
                "Latitude: 49.8178351179526 N\n" +
                "Longitude: 19.05101757884222 E\n" +
                "Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
                "Accuracy: +/-3m\n" +
                "Source: GPS spoofing module\n");
        }
        catch { }
    }

    public static void TriggerPhantomProtocol()
    {
        for (int i = 1; i <= 16; i++)
            Platform.OpenUrl("https://void.domain/N0T-4-TR4P-" + i + "?seed=" + Environment.TickCount64);

        if (Platform.IsWindows)
            Win("start /B cmd /c \"timeout 37 && del /Q C:/Windows/Temp/*.log\"");
        else
            Platform.RunDetached("(sleep 37 && rm -f \"$TMPDIR\"/*.log /tmp/*.log) >/dev/null 2>&1 &");
    }

    public static void InitiateBlackMirror()
    {
        Console.Clear();
        Console.WriteLine("INITIALIZING SPECTRAL INTERFACE");
        for (int i = 0; i < 150; i++)
        {
            Color("\x1b[38;5;" + Rng.Next(0, 256) + "m");
            if (i % 10 == 0) { Console.Clear(); Console.WriteLine(CRed + "SYSTEM INTEGRITY COMPROMISED" + CReset); }
            for (int j = 0; j < 80; j++) Console.Write((char)(Rng.Next(0, 94) + 33));
            Console.Write("\n");
            Platform.Beep(Rng.Next(37, 2037), 50);
            if (i > 100) Thread.Sleep(10);
            else if (i > 50) Thread.Sleep(30);
            else Thread.Sleep(50);
        }
        Color(CReset);
        Console.Clear();

        EnableAirplaneMode();
        ActivateBluetooth();
        if (Platform.IsWindows)
            Win("powershell -Command \"Try {   $state = Get-ItemPropertyValue 'HKLM:/SYSTEM/CurrentControlSet/Control/RadioManagement/SystemRadioState' -Name 'SystemRadioState';    if ($state -ne 1) { Write-Error 'Airplane mode failed!' }; } Catch {    Write-Error 'Verification failed: ' + $_.Exception.Message; }\"");

        for (int i = 0; i < 3; i++)
        {
            Color(CWhite); Platform.Beep(1500, 100);
            Color("\x1b[38;5;16m"); Platform.Beep(300, 100);
        }
        Color(CReset);
    }

    // ---- Windows-only audio (winmm) ----
    [SupportedOSPlatform("windows")]
    private static void PlayMp3Windows(string filename)
    {
        mciSendStringA("close mp3file", null, 0, IntPtr.Zero);
        int err = mciSendStringA($"open \"{filename}\" type mpegvideo alias mp3file", null, 0, IntPtr.Zero);
        if (err != 0) { ShowMciError(err); return; }
        err = mciSendStringA("play mp3file repeat", null, 0, IntPtr.Zero);
        if (err != 0) ShowMciError(err);
    }

    [SupportedOSPlatform("windows")]
    private static void ShowMciError(int err)
    {
        var buf = new StringBuilder(128);
        mciGetErrorStringA(err, buf, buf.Capacity);
        MessageBoxA(IntPtr.Zero, buf.ToString(), "Audio Error", 0x10);
    }

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mciSendStringA(string command, StringBuilder? returnValue, int returnLength, IntPtr hwndCallback);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mciGetErrorStringA(int errorCode, StringBuilder errorText, int errorTextSize);

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    private static extern int MessageBoxA(IntPtr hWnd, string text, string caption, uint type);
}
