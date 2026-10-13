using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Cara.Core;
using static Cara.Ui.ConsoleUi;

namespace Cara.Fx;

/// Persistent satellite windows (tr/bl/r): a CRT hacker desktop drawn on a
/// clipped cell canvas and driven by small files. Degrades to a solo window.
public static class Panes
{
    // ---------------- main side (public API unchanged) ----------------
    private static string? _dir;
    private static readonly List<Process> _kids = new();
    private static readonly string[] SlotIds = { "tr", "bl", "r" };
    public static bool Active { get; private set; }

    public static void Start()
    {
        if (Active) return;
        try
        {
            string? exe = Environment.ProcessPath;
            if (exe == null
                || exe.EndsWith("dotnet", StringComparison.OrdinalIgnoreCase)
                || exe.EndsWith("dotnet.exe", StringComparison.OrdinalIgnoreCase)) return;

            _dir = Path.Combine(Path.GetTempPath(), "cara_panes_" + Environment.ProcessId);
            Directory.CreateDirectory(_dir);
            int ppid = Environment.ProcessId;

            if (OperatingSystem.IsWindows()) StartWindows(exe, ppid);
            else StartLinux(exe, ppid);

            Active = _kids.Count > 0;
            if (Active) Idle(); else TryCleanup();
        }
        catch { Active = false; }
    }

    [SupportedOSPlatform("windows")]
    private static void StartWindows(string exe, int ppid)
    {
        int sw = GetSystemMetrics(0), sh = GetSystemMetrics(1);
        if (sw <= 0 || sh <= 0) return;
        foreach (string slot in SlotIds)
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo(exe, $"--pane {slot} \"{_dir}\" {ppid}") { UseShellExecute = true });
                if (p != null) _kids.Add(p);
            }
            catch { }
        }
        if (_kids.Count > 0) try { MoveWindow(GetConsoleWindow(), sw / 4, sh / 6, sw / 2, sh * 2 / 3, true); } catch { }
    }

    private static void StartLinux(string exe, int ppid)
    {
        var (sw, sh) = Platform.ScreenSize();
        foreach (string slot in SlotIds)
        {
            string title = "CARA::" + slot.ToUpperInvariant();
            string cmd = "\"" + exe + "\" --pane " + slot + " \"" + _dir + "\" " + ppid;
            var p = Platform.SpawnTerminal(title, cmd);
            if (p != null) _kids.Add(p);
        }
        if (_kids.Count > 0)
        {
            Thread.Sleep(700);
            foreach (string slot in SlotIds)
            {
                var (x, y, w, h) = LinuxRect(slot, sw, sh);
                Platform.MoveWindowByTitle("CARA::" + slot.ToUpperInvariant(), x, y, w, h);
            }
        }
    }

    private static (int x, int y, int w, int h) LinuxRect(string slot, int sw, int sh) => slot switch
    {
        "tr" => (sw * 2 / 3, 0, sw / 3, sh / 2),
        "bl" => (0, sh / 2, sw / 3, sh / 2),
        _ => (sw * 2 / 3, sh / 2, sw / 3, sh / 2),
    };

    public static void Stop()
    {
        if (!Active) return;
        Active = false;
        try { if (_dir != null) File.WriteAllText(Path.Combine(_dir, "stop.txt"), "1"); } catch { }
        Thread.Sleep(180);
        foreach (var p in _kids) { try { if (!p.HasExited) p.Kill(); } catch { } }
        _kids.Clear();
        TryCleanup();
    }

    private static void TryCleanup()
    {
        try { if (_dir != null && Directory.Exists(_dir)) Directory.Delete(_dir, true); } catch { }
    }

    private static void Write(string slot, params string[] lines)
    {
        if (!Active || _dir == null) return;
        try
        {
            string tmp = Path.Combine(_dir, slot + ".tmp");
            string dst = Path.Combine(_dir, slot + ".txt");
            File.WriteAllText(tmp, string.Join("\n", lines));
            File.Move(tmp, dst, true);
        }
        catch { }
    }

    public static void Glitch(int ms = 1200)
    {
        if (!Active || _dir == null) return;
        try { File.WriteAllText(Path.Combine(_dir, "glitch.txt"), DateTime.UtcNow.AddMilliseconds(ms).Ticks.ToString(CultureInfo.InvariantCulture)); }
        catch { }
    }

    public static void Idle()
    {
        Write("tr", "calendar");
        Write("r", "ascii me", "USER|" + UserTag(), "NODE|cara.exe", "CLR|OMEGA|||hot", "LINK|secure|||ok");
        Write("bl", "anim code");
    }

    public static void Terminal()
    {
        Write("tr", "stats", "TERMINAL", "@• ONLINE", "#TIME|clock|LOCAL",
              "MODE|hacker", "NODE|cara.exe", "LINK|up|||ok", "-", "CLR|OMEGA|||hot");
        Write("r", "anim wave");
        Write("bl", "anim scan");
    }

    public static void Dice(string foe, int hp, int max, int atk, int wave, int score)
    {
        string f = Fold(foe);
        Write("tr", "stats", "ENEMY", "@WAVE " + wave, "#HP|" + hp + "|OF " + max,
              "FOE|" + f + "|||hot",
              "HP|" + hp + "/" + max + "|" + hp + "|" + max + "|hp",
              "ATK|" + atk + "|" + Math.Min(atk, 20) + "|20|heat",
              "-", "WAVE|" + wave, "SCORE|" + score);
        Write("r", "ascii " + AsciiKey(foe), "NAME|" + f + "|||hot",
              "HP|" + hp + "/" + max + "|" + hp + "|" + max + "|hp");
        Write("bl", "anim bars");
    }

    public static void Reactor(int temp, int cool, int react, string status)
    {
        string s = Fold(status);
        Write("tr", "stats", "REACTOR", "@" + s, "#TEMP|" + temp + "|CELSIUS",
              "TEMP|" + temp + "/100|" + temp + "|100|heat",
              "COOL|" + cool + "/100|" + cool + "|100|cool",
              "REACT|" + react + "%|" + react + "|100",
              "~HIST|" + temp, "-", "STATE|" + s + "|||state");
        Write("r", "anim core " + temp + " " + cool + " " + react);
        Write("bl", "anim grid " + temp);
    }

    public static void CodeRunner(int pts, int lvl, int best)
    {
        int bm = Math.Max(best, 1);
        string st = best > 0 && pts > best ? "NEW RECORD" : "ACTIVE";
        Write("tr", "stats", "UPLINK", "@• ACTIVE", "#PTS|" + pts + "|SEGMENTS",
              "LVL|" + lvl + "|" + Math.Min(lvl, 10) + "|10|heat",
              "BEST|" + best + "|" + Math.Min(pts, bm) + "|" + bm + "|ok",
              "-", "STATE|" + st + "|||state");
        Write("r", "anim code net");
        Write("bl", "anim scan");
    }

    // ---- command reactions: side panels respond to what you type ----
    private static void Scene(string title, string meta, string[] trRows, string rPane, string blPane)
    {
        var tr = new List<string>(trRows.Length + 3) { "stats", title, "@" + meta };
        tr.AddRange(trRows);
        Write("tr", tr.ToArray());
        Write("r", rPane.Split('\n'));
        Write("bl", blPane.Split('\n'));
    }

    public static void Calc() => Scene("ARITHMETIC CORE", "• COMPUTE",
        new[] { "MODE|eval|||ok", "PREC|double", "-", "CLR|OMEGA|||hot" }, "anim bars", "anim scan");

    public static void Timer() => Scene("COUNTDOWN", "• ARMED",
        new[] { "CLOCK|running|||state", "-", "STATE|TICKING|||state" }, "anim core 60 40 80", "anim bars");

    public static void Secure(string kind) => Scene("CIPHER FORGE", "• " + Fold(kind).ToUpperInvariant(),
        new[] { "ENTROPY|high|||ok", "BITS|256|256|256|ok", "-", "RNG|csprng|||ok" }, "anim code net", "anim core 70 50 90");

    public static void Archive() => Scene("ARCHIVE", "• NOTES",
        new[] { "STORE|local|||ok", "-", "STATE|SAVED|||state" }, "anim code", "anim scan");

    public static void Host() => Scene("SYSTEM PROBE", "• SCAN",
        new[] { "HOST|" + NodeTag() + "|||ok", "USER|" + UserTag(), "-", "LINK|up|||ok" },
        "ascii me\nUSER|" + UserTag() + "|||ok\nNODE|cara.exe", "anim grid 40");

    public static void Index() => Scene("INDEX", "• MENU",
        new[] { "CMDS|ready|||ok", "-", "NODE|cara.exe" }, "ascii me\nUSER|" + UserTag() + "|||ok", "anim wave");

    public static void Voice() => Scene("VOICE CORE", "• SPEAKING",
        new[] { "OUT|audio|||ok", "-", "STATE|LIVE|||state" }, "anim wave", "anim bars");

    public static void Time()
    {
        Write("tr", "calendar");
        Write("r", "stats", "WORLD CLOCK", "@• LOCAL", "#TIME|clock|NOW", "ZONE|local", "-", "SYNC|ntp|||ok");
        Write("bl", "anim wave");
    }

    public static void Recover()
    {
        Write("tr", "stats", "GUARDIAN", "@• RESTORING", "WIFI|back|||ok", "RADIO|on|||ok", "-", "STATE|SAFE|||state");
        Write("r", "ascii me", "USER|" + UserTag() + "|||ok", "LINK|secure|||ok");
        Write("bl", "anim grid 20");
    }

    // ---- horror reactions for hidden codes and the game commands ----
    public static void Watch()
    {
        Write("tr", "stats", "SURVEILLANCE", "@EYES ON YOU",
            "SUBJECT|" + UserTag() + "|||hot", "CAM|ONLINE|||hot", "MATCH|98.7%|||hot", "-", "FEED|live|||hot");
        Write("r", "ascii wraith", "TARGET|" + UserTag() + "|||hot");
        Write("bl", "anim scan");
        Glitch(1800);
    }

    public static void Breach()
    {
        Write("tr", "stats", "INTRUSION", "@ACCESS GRANTED",
            "FIREWALL|DOWN|||hot", "SHELL|root|||hot", "-", "OWNED|" + NodeTag() + "|||hot");
        Write("r", "ascii daemon", "HOST|" + NodeTag() + "|||hot");
        Write("bl", "anim code net");
        Glitch(1800);
    }

    public static void Signal()
    {
        Write("tr", "stats", "DEAD SIGNAL", "@CARRIER 666.6",
            "SNR|falling|||hot", "EVP|CLASS A|||hot", "-", "LINK|lost|||hot");
        Write("r", "ascii generic", "SRC|UNKNOWN|||hot");
        Write("bl", "anim wave");
        Glitch(1800);
    }

    public static void Ritual()
    {
        Write("tr", "stats", "WELCOME", "@IT IS AWAKE",
            "SOUL|" + UserTag() + "|||hot", "DOOR|OPEN|||hot", "-", "RUN|no|||hot");
        Write("r", "ascii wraith", "YOU|" + UserTag() + "|||hot");
        Write("bl", "anim core 95 10 99");
        Glitch(4000);
    }

    private static string NodeTag()
    {
        try
        {
            string n = Fold(Environment.MachineName).ToUpperInvariant();
            if (n.Length > 12) n = n[..12];
            return n.Length > 0 ? n : "NODE";
        }
        catch { return "NODE"; }
    }

    private static string AsciiKey(string foe)
    {
        string f = foe.ToLowerInvariant();
        if (f.Contains("worm")) return "worm";
        if (f.Contains("root")) return "rootkit";
        if (f.Contains("troj")) return "trojan";
        if (f.Contains("daem")) return "daemon";
        if (f.Contains("wra")) return "wraith";
        if (f.Contains("rans")) return "ransom";
        if (f.Contains("b0t") || f.Contains("bot")) return "botnet";
        if (f.Contains("keyl")) return "keylog";
        return "generic";
    }

    private static string Fold(string s)
    {
        try
        {
            string n = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (char c in n)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) continue;
                char d = c;
                if (d == '\u0142') d = 'l';
                else if (d == '\u0141') d = 'L';
                else if (d == '|') d = '/';
                if (d < 0x20 || d > 0x7E) d = '?';
                sb.Append(d);
            }
            return sb.ToString();
        }
        catch { return "?"; }
    }

    private static string UserTag()
    {
        try
        {
            string u = Fold(Environment.UserName).ToUpperInvariant();
            if (u.Length > 12) u = u[..12];
            return u.Length > 0 ? u : "OPERATOR";
        }
        catch { return "OPERATOR"; }
    }

    // ---------------- satellite side ----------------
    private const byte Ink = 16, D0 = 52, D1 = 88, D2 = 124, D3 = 160, D4 = 196, D5 = 203, Wh = 231;
    private const byte G0 = 236, G1 = 238, G2 = 244, Cy1 = 30, Cy2 = 37, Cy3 = 51, Gn2 = 34, Gn3 = 46, Amb = 214;
    private static readonly bool Fancy = true;

    private static byte Dim(byte c) => (byte)(c switch
    {
        231 => 250, 210 => 203, 203 => 196, 196 => 160, 160 => 124, 124 => 88, 88 => 52,
        250 => 244, 244 => 240, 240 => 238, 238 => 236,
        51 => 37, 37 => 30, 30 => 23, 46 => 34, 34 => 28, 214 => 172, _ => c,
    });
    private static byte Lift(byte c) => (byte)(c switch
    {
        52 => 88, 88 => 124, 124 => 160, 160 => 196, 196 => 203, 203 => 210, 210 => 231,
        236 => 238, 238 => 240, 240 => 244, 244 => 250, 250 => 231,
        23 => 30, 30 => 37, 37 => 51, 28 => 34, 34 => 46, 172 => 214, _ => c,
    });

    private static char Safe(char c)
    {
        if (c >= 0x20 && c <= 0x7E) return c;
        switch (c)
        {
            case '\u2500': case '\u2502': case '\u250c': case '\u2510': case '\u2514': case '\u2518':
            case '\u251c': case '\u2524': case '\u252c': case '\u2534': case '\u253c':
            case '\u2588': case '\u2592': case '\u2591': case '\u2022':
                return c;
            case '\u256d': return Fancy ? c : '\u250c';
            case '\u256e': return Fancy ? c : '\u2510';
            case '\u256f': return Fancy ? c : '\u2518';
            case '\u2570': return Fancy ? c : '\u2514';
            case '\u2574': case '\u2576': return Fancy ? c : '\u2500';
            case '\u2575': case '\u2577': return Fancy ? c : '\u2502';
        }
        return '?';
    }

    private sealed class Canvas
    {
        public readonly int W, H;
        private readonly char[] _ch; private readonly byte[] _fg, _bg; private readonly bool[] _bo;
        public Canvas(int w, int h)
        {
            W = Math.Max(1, w); H = Math.Max(1, h);
            int n = W * H; _ch = new char[n]; _fg = new byte[n]; _bg = new byte[n]; _bo = new bool[n];
            Clear();
        }
        public void Clear() { Array.Fill(_ch, ' '); Array.Fill(_fg, (byte)244); Array.Fill(_bg, (byte)0); Array.Fill(_bo, false); }
        public bool In(int x, int y) => (uint)x < (uint)W && (uint)y < (uint)H;
        public void Put(int x, int y, char c, byte fg, byte bg = 0, bool bold = false)
        {
            if (!In(x, y)) return;
            int i = y * W + x; _ch[i] = Safe(c); _fg[i] = fg; _bg[i] = bg; _bo[i] = bold;
        }
        public int Text(int x, int y, string s, byte fg, byte bg = 0, bool bold = false)
        { foreach (char c in s) Put(x++, y, c, fg, bg, bold); return x; }
        public char Ch(int x, int y) => In(x, y) ? _ch[y * W + x] : ' ';
        public byte Fg(int x, int y) => In(x, y) ? _fg[y * W + x] : (byte)0;
        public byte Bg(int x, int y) => In(x, y) ? _bg[y * W + x] : (byte)0;
        public void SetFg(int x, int y, byte c) { if (In(x, y)) _fg[y * W + x] = c; }
        public void SetBg(int x, int y, byte c) { if (In(x, y)) _bg[y * W + x] = c; }
        public string Flush()
        {
            var sb = new StringBuilder(W * H * 6);
            sb.Append("\x1b[?25l");
            for (int y = 0; y < H; y++)
            {
                sb.Append("\x1b[").Append(y + 1).Append(";1H");
                int cf = -1, cb = -1; bool cbo = false;
                for (int x = 0; x < W; x++)
                {
                    int i = y * W + x;
                    if (_fg[i] != cf || _bg[i] != cb || _bo[i] != cbo)
                    {
                        cf = _fg[i]; cb = _bg[i]; cbo = _bo[i];
                        sb.Append(cbo ? "\x1b[0;1;38;5;" : "\x1b[0;38;5;").Append(cf)
                          .Append(";48;5;").Append(cb == 0 ? 16 : cb).Append('m');
                    }
                    sb.Append(_ch[i]);
                }
                sb.Append("\x1b[0;48;5;16m\x1b[K");
            }
            sb.Append("\x1b[0;48;5;16m\x1b[J\x1b[0m");
            return sb.ToString();
        }
    }

    // per-pane animation state (each satellite is its own process)
    private static int _frame;
    private static string _curMode = "\0";
    private static readonly Random Rng = new();
    private static DateTime _paneStart;
    private static readonly Dictionary<string, double> _disp = new();
    private static readonly Dictionary<string, double[]> _hist = new();
    private static readonly List<(string t, string id, string task, string st, int shown)> _log = new();
    private static int _logAdds;
    private static double[] _barLvl = Array.Empty<double>(), _barPk = Array.Empty<double>();
    private static int[] _barHold = Array.Empty<int>();
    private static int[] _gridVal = Array.Empty<int>(); private static byte[] _gridAge = Array.Empty<byte>();
    private static int _scanOff = 0x0A3EF8;
    private static long _glUntil, _glStart;

    public static void RunPane(string slot, string dir, int ppid)
    {
        try { Console.OutputEncoding = Encoding.UTF8; } catch { }
        EnableVt(); HideCur();
        try { Console.Title = "CARA::" + slot.ToUpperInvariant(); } catch { }
        if (OperatingSystem.IsWindows()) { try { PositionSelf(slot); } catch { } }
        try { Console.Write("\x1b[?1049h\x1b[?7l\x1b[2J"); } catch { }
        _paneStart = DateTime.Now;

        List<string> state = new() { slot == "tr" ? "calendar" : slot == "bl" ? "anim code" : "anim wave" };
        int pw = -1, ph = -1;
        DateTime lastCheck = DateTime.Now;

        while (true)
        {
            if (dir.Length > 0)
            {
                try { if (File.Exists(Path.Combine(dir, "stop.txt"))) break; } catch { }
                var s = TryRead(Path.Combine(dir, slot + ".txt"));
                if (s != null && s.Count > 0) state = s;
            }
            if ((DateTime.Now - lastCheck).TotalSeconds > 1)
            {
                lastCheck = DateTime.Now;
                if (ppid > 0 && !ParentAlive(ppid)) break;
            }

            var sw = Stopwatch.StartNew();
            try
            {
                int w = SafeW(), h = SafeH();
                if (w != pw || h != ph) { pw = w; ph = h; try { Console.Write("\x1b[2J"); } catch { } }
                var cv = new Canvas(w - 1, h - 1);
                bool glitch = GlitchActive(dir);
                RenderFrame(cv, slot, state, glitch);
                try { Console.Out.Write(cv.Flush()); Console.Out.Flush(); } catch { }
            }
            catch { }
            _frame++;
            int spent = (int)sw.ElapsedMilliseconds;
            Thread.Sleep(Math.Clamp(83 - spent, 15, 83));
        }

        try { Console.Write("\x1b[?7h\x1b[?1049l"); } catch { }
        ShowCur();
    }

    private static void RenderFrame(Canvas cv, string slot, List<string> state, bool glitch)
    {
        int C = cv.W, R = cv.H;
        string mode = state.Count > 0 ? state[0] : "idle";
        if (mode != _curMode) { _curMode = mode; _disp.Clear(); _hist.Clear(); _log.Clear(); _logAdds = 0; }

        string[] tok = mode.Split(' ');
        string kind = tok[0];
        int X0 = 2, X1 = C - 4, Y0 = 1, Y1 = R - 3;
        int CW = X1 - X0 + 1, B = Y1 - Y0 + 1;
        string label = "CARA"; string meta = "";

        if (C < 20 || R < 6)
        {
            ChromeBar(cv, C, "CARA", "");
            cv.Text(Math.Max(0, C / 2 - 2), R / 2, "CARA", D3, 0, true);
            return;
        }

        if (kind == "calendar") { label = "CALENDAR"; meta = DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture); DrawCalendar(cv, X0, Y0, CW, B); }
        else if (kind == "stats") { label = state.Count > 1 ? state[1] : "DATA"; meta = DrawStats(cv, X0, Y0, CW, B, state); }
        else if (kind == "ascii") { string key = tok.Length > 1 ? tok[1] : "generic"; label = key == "me" ? "ME" : "TARGET"; meta = key == "me" ? "• ONLINE" : key.ToUpperInvariant() + " • LOCK"; DrawAscii(cv, X0, Y0, CW, B, key, state); }
        else { (label, meta) = DrawAnim(cv, X0, Y0, CW, B, tok); }

        CrtLayer(cv, C, R, kind);
        ChromeBar(cv, C, label, meta);
        ChromeFrame(cv, C, R, slot);

        if (glitch) GlitchOverlay(cv, C, R, slot, ref label);
    }

    // ---------------- chrome ----------------
    private static void ChromeBar(Canvas cv, int C, string label, string meta)
    {
        for (int x = 0; x < C; x++) cv.Put(x, 0, ' ', Ink, D3);
        string lab = label.ToUpperInvariant();
        if (lab.Length > C - 8) lab = lab[..Math.Max(0, C - 8)];
        cv.Text(1, 0, lab, Ink, D3, true);
        // [x]
        int bx = C - 4;
        cv.Put(bx, 0, '[', D4, D0, true); cv.Put(bx + 1, 0, 'x', D4, D0, true); cv.Put(bx + 2, 0, ']', D4, D0, true);
        if (meta.Length > 0)
        {
            bool bullet = meta.StartsWith("• ");
            string m = meta;
            int mx = C - 6 - m.Length;
            if (mx > lab.Length + 3)
            {
                for (int i = 0; i < m.Length; i++)
                {
                    char ch = m[i];
                    byte fg = D0;
                    if (bullet && i == 0) fg = ((_frame / 8) % 2 == 0) ? Wh : D0;
                    cv.Put(mx + i, 0, ch, fg, D3);
                }
            }
        }
    }

    private static void ChromeFrame(Canvas cv, int C, int R, string slot)
    {
        for (int y = 1; y <= R - 2; y++)
        {
            byte fg = (y % 2 == 1) ? D3 : D2;
            cv.Put(0, y, '\u2502', fg); cv.Put(C - 1, y, '\u2502', fg);
        }
        int by = R - 1;
        cv.Put(0, by, '\u2514', D3);
        for (int x = 1; x < C - 1; x++) cv.Put(x, by, '\u2500', D3);
        cv.Put(C - 1, by, '\u2518', D3);
        if (C >= 26)
        {
            string foot = " CARA//" + slot.ToUpperInvariant() + " ";
            cv.Text(3, by, foot, D1);
            int k = (_frame / 2) % 6;
            for (int i = 0; i < 4; i++)
            {
                int d = (k - i + 6) % 6; int cx = C - 7 + i;
                if (d == 0) cv.Put(cx, by, '\u2588', D4);
                else if (d == 1) cv.Put(cx, by, '\u2592', D3);
                else if (d == 2) cv.Put(cx, by, '\u2591', D2);
            }
        }
    }

    private static void CrtLayer(Canvas cv, int C, int R, string kind)
    {
        bool anim = kind == "anim";
        if (anim)
            for (int y = 1; y <= R - 2; y++)
                if (y % 2 == 0)
                    for (int x = 1; x < C - 1; x++)
                        if (cv.Bg(x, y) == 0) cv.SetFg(x, y, Dim(cv.Fg(x, y)));
        int band = 1 + (_frame / 2) % (R + 18);
        if (band >= 1 && band <= R - 2)
        {
            for (int x = 1; x < C - 1; x++)
                if (cv.Bg(x, band) == 0) cv.SetFg(x, band, Lift(cv.Fg(x, band)));
            cv.SetFg(0, band, D5); cv.SetFg(C - 1, band, D5);
        }
        if (anim)
            for (int n = 0; n < 2; n++)
            {
                int x = 1 + Rng.Next(Math.Max(1, C - 2)), y = 1 + Rng.Next(Math.Max(1, R - 2));
                if (cv.Ch(x, y) == ' ' && cv.Bg(x, y) == 0) cv.Put(x, y, '\u2591', D0);
            }
    }

    // ---------------- payload parse + typography ----------------
    private sealed class Field { public string Label = "", Val = ""; public int Cur = -1, Max = -1; public string Tone = ""; }

    private static (List<Field> rows, string meta, string hero, string heroVal, string heroUnit) ParseStats(List<string> state)
    {
        var rows = new List<Field>(); string meta = "• LIVE", hero = "", hv = "", hu = "";
        for (int i = 2; i < state.Count; i++)
        {
            string ln = state[i];
            if (ln.Length == 0) continue;
            if (ln[0] == '@') { meta = ln[1..]; continue; }
            if (ln[0] == '#') { var p = ln[1..].Split('|'); hero = p.Length > 0 ? p[0] : ""; hv = p.Length > 1 ? p[1] : ""; hu = p.Length > 2 ? p[2] : ""; continue; }
            if (ln == "-") { rows.Add(new Field { Label = "-" }); continue; }
            if (ln[0] == '~') { var p = ln[1..].Split('|'); var f = new Field { Label = "~" + (p.Length > 0 ? p[0] : ""), Val = p.Length > 1 ? p[1] : "0" }; rows.Add(f); continue; }
            rows.Add(ParseField(ln));
        }
        return (rows, meta, hero, hv, hu);
    }

    private static Field ParseField(string ln)
    {
        var f = new Field();
        if (ln.Contains('|'))
        {
            var p = ln.Split('|');
            f.Label = p[0]; f.Val = p.Length > 1 ? p[1] : "";
            if (p.Length > 2 && int.TryParse(p[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out int cu)) f.Cur = cu;
            if (p.Length > 3 && int.TryParse(p[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out int mx)) f.Max = mx;
            f.Tone = p.Length > 4 ? p[4] : "";
        }
        else
        {
            int sp = ln.IndexOf("  ", StringComparison.Ordinal);
            if (sp < 0) { f.Label = ln; } else { f.Label = ln[..sp]; f.Val = ln[(sp + 2)..].TrimStart(); }
        }
        return f;
    }

    private static string[] SevenSeg(char c) => c switch
    {
        '0' => new[] { "\u250c\u2500\u2510", "\u2502 \u2502", "\u2514\u2500\u2518" },
        '1' => new[] { "\u2574\u2510 ", " \u2502 ", "\u2574\u2534\u2574" },
        '2' => new[] { "\u2574\u2500\u2510", "\u250c\u2500\u2518", "\u2514\u2500\u2574" },
        '3' => new[] { "\u2574\u2500\u2510", " \u2500\u2524", "\u2574\u2500\u2518" },
        '4' => new[] { "\u2577 \u2577", "\u2514\u2500\u2524", "  \u2575" },
        '5' => new[] { "\u250c\u2500\u2574", "\u2514\u2500\u2510", "\u2576\u2500\u2518" },
        '6' => new[] { "\u250c\u2500\u2574", "\u251c\u2500\u2510", "\u2514\u2500\u2518" },
        '7' => new[] { "\u2574\u2500\u2510", "  \u2502", "  \u2575" },
        '8' => new[] { "\u250c\u2500\u2510", "\u251c\u2500\u2524", "\u2514\u2500\u2518" },
        '9' => new[] { "\u250c\u2500\u2510", "\u2514\u2500\u2524", "\u2576\u2500\u2518" },
        '-' => new[] { "   ", "\u2576\u2500\u2574", "   " },
        _ => new[] { "   ", "   ", "   " },
    };

    private static byte ToneColor(string tone, double r) => tone switch
    {
        "hot" => D5,
        "dim" => G2,
        "ok" => Gn2,
        "cool" => Cy2,
        "hp" => r > 0.5 ? D4 : r > 0.25 ? D5 : (((_frame / 3) % 2 == 0) ? Wh : D4),
        "heat" => r < 0.5 ? D2 : r < 0.8 ? D4 : (((_frame / 3) % 2 == 0) ? D5 : Wh),
        _ => D4,
    };

    private static void Gauge(Canvas cv, int gx, int gy, int gw, double r, string tone, string label)
    {
        r = Math.Clamp(r, 0, 1);
        double target = r * gw;
        double disp = _disp.TryGetValue(label, out double d) ? d : target;
        if (target > disp) disp += (target - disp) * 0.35;
        else if (target < disp) disp -= Math.Max(0.25, (disp - target) * 0.12);
        if (Math.Abs(disp - target) < 0.05) disp = target;
        _disp[label] = disp;
        int fill = (int)Math.Floor(Math.Min(target, disp));
        byte fc = ToneColor(tone, r);
        byte head = tone == "ok" ? Gn3 : tone == "cool" ? Cy3 : D5;
        int shim = _frame % Math.Max(1, fill + 8);
        for (int i = 0; i < gw; i++)
        {
            if (i < fill) { byte c = (i == fill - 1) ? head : (i == shim ? Lift(fc) : fc); cv.Put(gx + i, gy, '\u2588', c); }
            else if (i < disp) cv.Put(gx + i, gy, '\u2588', D5);
            else cv.Put(gx + i, gy, '\u2591', D0);
        }
    }

    private static int DrawFieldRow(Canvas cv, int x0, int x1, int y, Field f)
    {
        if (f.Label == "-") { cv.Put(0, y, '\u251c', D3); for (int x = 1; x < cv.W - 1; x++) cv.Put(x, y, '\u2500', D1); cv.Put(cv.W - 1, y, '\u2524', D3); return y + 1; }
        if (f.Label.StartsWith("~")) { DrawSpark(cv, x0, x1, y, f.Label[1..], f.Val); return y + 1; }
        int lw = 6;
        cv.Text(x0, y, Pad(f.Label.ToUpperInvariant(), lw), D2);
        int vx = x0 + lw + 1, vw = 8;
        bool numeric = f.Val.Length > 0 && char.IsDigit(f.Val[0]);
        string v = f.Val.Length > vw ? f.Val[..vw] : f.Val;
        if (f.Tone == "state") { DrawBadge(cv, vx, y, f.Val); }
        else if (numeric) cv.Text(vx + vw - v.Length, y, v, ToneColor(f.Tone, 1));
        else cv.Text(vx, y, v, ToneColor(f.Tone, 1));
        if (f.Max > 0)
        {
            int gx = vx + vw + 1, gw = Math.Min(26, x1 - gx + 1 - 5);
            if (gw >= 6)
            {
                double r = (double)Math.Clamp(f.Cur, 0, f.Max) / f.Max;
                Gauge(cv, gx, y, gw, r, f.Tone, f.Label);
                cv.Text(gx + gw + 1, y, Pad3((int)(r * 100)) + "%", G2);
            }
        }
        return y + 1;
    }

    private static void DrawBadge(Canvas cv, int x, int y, string text)
    {
        string t = " " + text + " ";
        byte fg, bg = 0; bool bold = false;
        string u = text.ToUpperInvariant();
        if (u.Contains("CRIT") || u.Contains("MELT")) { fg = Ink; bg = ((_frame / 3) % 2 == 0) ? D4 : D2; bold = true; }
        else if (u.Contains("RIS") || u.Contains("WARN")) { fg = Amb; }
        else if (u.Contains("NEW")) { fg = Ink; bg = Amb; bold = true; }
        else { fg = Gn2; }
        for (int i = 0; i < t.Length; i++) cv.Put(x + i, y, t[i], fg, bg, bold);
    }

    private static void DrawSpark(Canvas cv, int x0, int x1, int y, string label, string val)
    {
        cv.Text(x0, y, Pad(label.ToUpperInvariant(), 6), D2);
        double[] h = _hist.TryGetValue(label, out var hh) ? hh : new double[64];
        if (_frame % 3 == 0 && double.TryParse(val, NumberStyles.Float, CultureInfo.InvariantCulture, out double v))
        { for (int i = 0; i < h.Length - 1; i++) h[i] = h[i + 1]; h[^1] = v; }
        _hist[label] = h;
        int sx = x0 + 8, n = x1 - sx + 1;
        for (int i = 0; i < n; i++)
        {
            double sv = h[h.Length - n + i >= 0 ? h.Length - n + i : 0];
            int L = (int)Math.Round(Math.Clamp(sv / 100.0, 0, 1) * 7);
            (char ch, byte fg) = L switch
            {
                0 => (' ', D0), 1 => ('\u2591', D1), 2 => ('\u2591', D2), 3 => ('\u2592', D2),
                4 => ('\u2592', D3), 5 => ('\u2588', D3), 6 => ('\u2588', D4), _ => ('\u2588', D5),
            };
            cv.Put(sx + i, y, ch, fg);
        }
    }

    private static void DrawHero(Canvas cv, int x0, int y, string label, string val, string unit)
    {
        string digits = val;
        if (val == "clock") { var n = DateTime.Now; digits = n.ToString("HH", CultureInfo.InvariantCulture) + ":" + n.ToString("mm", CultureInfo.InvariantCulture); }
        int cx = x0;
        foreach (char c in digits)
        {
            if (c == ':')
            {
                bool on = (_frame / 8) % 2 == 0;
                cv.Put(cx, y, on ? '\u2022' : ' ', D4); cv.Put(cx, y + 2, on ? '\u2022' : ' ', D4); cx += 2; continue;
            }
            var g = SevenSeg(c);
            for (int r = 0; r < 3; r++) cv.Text(cx, y + r, g[r], D4);
            cx += 4;
        }
        cv.Text(cx + 1, y, label, D2);
        cv.Text(cx + 1, y + 2, unit, G2);
    }

    private static string DrawStats(Canvas cv, int x0, int y0, int CW, int B, List<string> state)
    {
        var (rows, meta, hero, hv, hu) = ParseStats(state);
        int x1 = x0 + CW - 1, y = y0 + 1;
        if (hero.Length > 0 && y + 3 <= y0 + B) { DrawHero(cv, x0, y, hero, hv, hu); y += 4; }
        foreach (var f in rows)
        {
            if (y > y0 + B - 1) break;
            y = DrawFieldRow(cv, x0, x1, y, f);
        }
        return meta;
    }

    // ---------------- calendar ----------------
    private static readonly string[][] Big = {
        new[]{"###","# #","# #","# #","###"}, new[]{" # ","## "," # "," # ","###"},
        new[]{"###","  #","###","#  ","###"}, new[]{"###","  #"," ##","  #","###"},
        new[]{"# #","# #","###","  #","  #"}, new[]{"###","#  ","###","  #","###"},
        new[]{"###","#  ","###","# #","###"}, new[]{"###","  #","  #"," # "," # "},
        new[]{"###","# #","###","# #","###"}, new[]{"###","# #","###","  #","###"},
    };

    private static void DrawCalendar(Canvas cv, int x0, int y0, int CW, int B)
    {
        DateTime now = DateTime.Now;
        int y = y0 + 1;
        bool dbl = CW >= 30;
        string dd = now.Day.ToString("00", CultureInfo.InvariantCulture);
        byte[] rowCol = { D5, D4, D4, D3, D2 };
        int cell = dbl ? 6 : 3, gap = dbl ? 2 : 1;
        for (int d = 0; d < 2; d++)
        {
            var bmp = Big[dd[d] - '0'];
            int dx = x0 + d * (cell + gap);
            for (int r = 0; r < 5; r++)
                for (int cxi = 0; cxi < 3; cxi++)
                    if (bmp[r][cxi] == '#')
                    {
                        if (dbl) { cv.Put(dx + cxi * 2, y + r, '\u2588', rowCol[r]); cv.Put(dx + cxi * 2 + 1, y + r, '\u2588', rowCol[r]); }
                        else cv.Put(dx + cxi, y + r, '\u2588', rowCol[r]);
                    }
        }
        int bigW = 2 * cell + gap, ix = x0 + bigW + 4;
        cv.Text(ix, y, now.ToString("MMMM", CultureInfo.InvariantCulture).ToUpperInvariant(), D4, 0, true);
        cv.Text(ix, y + 1, now.Year.ToString(CultureInfo.InvariantCulture), G2);
        for (int i = 0; i < 10; i++) cv.Put(ix + i, y + 2, '\u2500', D1);
        cv.Text(ix, y + 3, now.ToString("dddd", CultureInfo.InvariantCulture).ToUpperInvariant(), D3);
        cv.Text(ix, y + 4, "WK " + ISOWeek.GetWeekOfYear(now) + " DAY " + now.DayOfYear, G2);

        int gy = y + 6;
        cv.Text(x0, gy, "MO TU WE TH FR SA SU", D2);
        int days = DateTime.DaysInMonth(now.Year, now.Month);
        int lead = ((int)new DateTime(now.Year, now.Month, 1).DayOfWeek + 6) % 7;
        int col = lead; int wy = gy + 1;
        for (int dd2 = 1; dd2 <= days; dd2++)
        {
            int cx = x0 + col * 3;
            bool today = dd2 == now.Day;
            string s = dd2.ToString().PadLeft(2);
            if (today)
            {
                byte bg = ((_frame / 8) % 2 == 0) ? D4 : D3;
                cv.Put(cx, wy, s[0], Ink, bg, true);
                cv.Put(cx + 1, wy, s[1], Ink, bg, true);
                cv.Put(cx + 2, wy, ' ', Ink, bg);
            }
            else
            {
                byte c = dd2 < now.Day ? D1 : (col >= 5 ? D2 : D3);
                cv.Text(cx, wy, s, c);
            }
            col++;
            if (col == 7) { col = 0; wy++; }
        }
        int fy = gy + 8;
        if (fy <= y0 + B - 1)
        {
            double yr = (double)now.DayOfYear / (DateTime.IsLeapYear(now.Year) ? 366 : 365);
            cv.Text(x0, fy, now.Year.ToString(CultureInfo.InvariantCulture), G2);
            int gx = x0 + 6, gw = Math.Min(24, CW - 12);
            if (gw >= 6) { Gauge(cv, gx, fy, gw, yr, "", "yearbar"); cv.Text(gx + gw + 1, fy, Pad3((int)(yr * 100)) + "%", G2); }
        }
    }

    // ---------------- ascii ----------------
    private static void DrawAscii(Canvas cv, int x0, int y0, int CW, int B, string key, List<string> state)
    {
        string[] art = Art.TryGetValue(key, out var a) ? a : Art["generic"];
        int artW = 0; foreach (var l in art) artW = Math.Max(artW, l.Length);
        int artH = art.Length;
        int ax = x0 + (CW - artW) / 2, ay = y0 + 2;
        bool brownout = Rng.Next(50) == 0;
        for (int r = 0; r < artH; r++)
        {
            string line = art[r];
            int shift = Rng.Next(16) == 0 ? 1 : 0;
            for (int cxi = 0; cxi < line.Length; cxi++)
            {
                char ch = line[cxi];
                if (ch == ' ') continue;
                byte fg;
                if (ch == 'o' || ch == '@' || ch == '*' || ch == '0' || ch == 'O') fg = Wh;
                else if (ch == 'V' || ch == '^' || ch == '$' || ch == '!') fg = D5;
                else fg = (r % 2 == 0) ? D3 : D2;
                if (brownout) fg = D1;
                bool eye = fg == Wh;
                cv.Put(ax + cxi + shift, ay + r, ch, fg, 0, eye);
            }
        }
        if (key != "me")
        {
            int m = (_frame / 8) % 2;
            int L = ax - 2 - m, Rr = ax + artW + 1 + m, T = ay - 1, Bt = ay + artH;
            byte rc = ((_frame / 8) % 2 == 0) ? D5 : D3;
            if (L >= 1 && Rr <= cv.W - 2)
            {
                cv.Put(L, T, '\u250c', rc); cv.Put(L + 1, T, '\u2500', rc); cv.Put(Rr - 1, T, '\u2500', rc); cv.Put(Rr, T, '\u2510', rc);
                cv.Put(L, Bt, '\u2514', rc); cv.Put(L + 1, Bt, '\u2500', rc); cv.Put(Rr - 1, Bt, '\u2500', rc); cv.Put(Rr, Bt, '\u2518', rc);
            }
        }
        int iy = ay + artH + 2, x1 = x0 + CW - 1;
        for (int i = 1; i < state.Count; i++)
        {
            if (iy > y0 + B - 1) break;
            if (state[i].Length == 0) continue;
            iy = DrawFieldRow(cv, x0, x1, iy, ParseField(state[i]));
        }
        if (key == "me" && iy <= y0 + B - 1)
        {
            var up = DateTime.Now - _paneStart;
            cv.Text(x0, iy, Pad("UP", 6), D2);
            cv.Text(x0 + 7, iy, up.ToString(@"hh\:mm\:ss", CultureInfo.InvariantCulture), D4);
        }
    }

    // ---------------- anims ----------------
    private static (string, string) DrawAnim(Canvas cv, int x0, int y0, int CW, int B, string[] tok)
    {
        string kind = tok.Length > 1 ? tok[1] : "wave";
        switch (kind)
        {
            case "code": return DrawCode(cv, x0, y0, CW, B, tok.Length > 2 && tok[2] == "net");
            case "bars": return DrawBars(cv, x0, y0, CW, B);
            case "scan": return DrawScan(cv, x0, y0, CW, B);
            case "grid": return DrawGrid(cv, x0, y0, CW, B, tok.Length > 2 && int.TryParse(tok[2], out int t) ? t : 30);
            case "core": return DrawCore(cv, x0, y0, CW, B, tok);
            default: return DrawWave(cv, x0, y0, CW, B);
        }
    }

    private static (string, string) DrawWave(Canvas cv, int x0, int y0, int CW, int B)
    {
        int gut = CW >= 32 ? 4 : 0;
        int px0 = x0 + gut, pw = CW - gut, ph = B;
        double t = _frame;
        double A = 0.85 + 0.15 * Math.Sin(t * 0.05);
        double S(double u) => A * (0.55 * Math.Sin(0.19 * u) + 0.30 * Math.Sin(0.051 * u + 1.3) + 0.15 * Math.Sin(0.83 * u + 0.4));
        int Row(double v) => (int)Math.Round((1 - (Math.Clamp(v, -1, 1) + 1) / 2) * (ph - 1));
        for (int x = 0; x < pw; x++)
        {
            int y0r = Row(S(x + t)), y1r = Row(S(x + 1 + t));
            int cx = px0 + x;
            byte col = (x == pw - 1) ? Wh : D4;
            if (y0r == y1r) cv.Put(cx, y0 + y0r, '\u2500', col);
            else if (y1r < y0r) { cv.Put(cx, y0 + y0r, '\u256f', col); cv.Put(cx, y0 + y1r, '\u256d', col); for (int yy = y1r + 1; yy < y0r; yy++) cv.Put(cx, y0 + yy, '\u2502', col); }
            else { cv.Put(cx, y0 + y0r, '\u256e', col); cv.Put(cx, y0 + y1r, '\u2570', col); for (int yy = y0r + 1; yy < y1r; yy++) cv.Put(cx, y0 + yy, '\u2502', col); }
            for (int yy = Math.Max(y0r, y1r) + 1; yy < ph; yy++) cv.Put(cx, y0 + yy, (yy % 2 == 0) ? '\u2591' : ' ', D0);
        }
        if (gut >= 4)
        {
            cv.Text(x0, y0, "+1", G1); cv.Put(x0 + 3, y0, '\u2524', D1);
            cv.Text(x0 + 1, y0 + ph / 2, "0", G1); cv.Put(x0 + 3, y0 + ph / 2, '\u2524', D1);
            cv.Text(x0, y0 + ph - 1, "-1", G1); cv.Put(x0 + 3, y0 + ph - 1, '\u2524', D1);
        }
        return ("SIGNAL", "CH1 48.0kHz");
    }

    private static readonly string[] Tasks = {
        "Service Host: DHCP Client", "Service Host: DNS Client", "Service Host: Event Log",
        "Service Host: Local Service", "Service Host: Network List", "Service Host: Local System",
        "Application Frame Host", "System Settings Broker", "Session Manager", "Runtime Broker",
        "svchost.exe -k netsvcs", "lsass.exe", "audiodg.exe", "conhost.exe 0x4", "cara.exe --pane",
        "watcher.sys", "evprecorder", "signal_decay_monitor", "UNKNOWN PROCESS", "Not Found",
    };
    private static readonly string[] Stat = { "OK", "OK", "OK", "RUN", "WAIT", "ERR" };

    private static (string, string) DrawCode(Canvas cv, int x0, int y0, int CW, int B, bool net)
    {
        if (_frame % Rng.Next(2, 7) == 0)
        {
            string id = Rng.Next(4) == 0 ? "0x" + Rng.Next(0x1000, 0xFFFF).ToString("X4") : Rng.Next(100000, 999999).ToString();
            string task = net ? ("TX " + Rng.Next(0x1000, 0xFFFF).ToString("X4") + " 10.0." + Rng.Next(256) + "." + Rng.Next(256)) : Tasks[Rng.Next(Tasks.Length)];
            _log.Add((DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture), id, task, Stat[Rng.Next(Stat.Length)], 0));
            _logAdds++;
            if (_log.Count > 80) _log.RemoveRange(0, _log.Count - 80);
        }
        cv.Text(x0, y0, "TIME", D5); cv.Text(x0 + 10, y0, "ID", D5); cv.Text(x0 + 18, y0, "TASK", D5);
        for (int x = 0; x < CW; x++) cv.SetBg(x0 + x, y0, D0);
        int rows = B - 1, n = Math.Min(rows, _log.Count);
        bool stCol = CW >= 40;
        for (int i = 0; i < n; i++)
        {
            var e = _log[_log.Count - n + i]; int ry = y0 + 1 + i; bool newest = i == n - 1;
            cv.Text(x0, ry, e.t, newest ? Wh : G2);
            cv.Text(x0 + 10, ry, e.id.Length > 6 ? e.id[..6] : e.id, newest ? D5 : D2);
            string task = e.task; int tw = CW - 18 - (stCol ? 5 : 0); if (task.Length > tw) task = task[..Math.Max(0, tw)];
            cv.Text(x0 + 18, ry, task, newest ? Wh : D3);
            if (stCol) { byte sc = e.st == "OK" ? Gn2 : e.st == "RUN" ? D4 : e.st == "ERR" ? Wh : Amb; cv.Text(x0 + CW - 4, ry, e.st, sc); }
        }
        return (net ? "LOG" : "LOG", _logAdds + (net ? " PKT" : " PROC"));
    }

    private static (string, string) DrawBars(Canvas cv, int x0, int y0, int CW, int B)
    {
        int bw = CW < 16 ? 1 : 2, step = bw + 1, N = Math.Max(4, (CW + 1) / 3);
        if (_barLvl.Length != N) { _barLvl = new double[N]; _barPk = new double[N]; _barHold = new int[N]; }
        int ph = B - 2; if (ph < 2) ph = B;
        for (int b = 0; b < N; b++)
        {
            double env = 1 - 0.5 * b / N;
            double tgt = Math.Clamp(0.12 + 0.45 * env * (0.6 + 0.4 * Math.Sin(_frame * 0.37 + b * 0.9)) + Rng.NextDouble() * 0.2, 0, 1);
            _barLvl[b] = tgt > _barLvl[b] ? _barLvl[b] + (tgt - _barLvl[b]) * 0.6 : _barLvl[b] - 0.08;
            if (_barLvl[b] < 0) _barLvl[b] = 0;
            if (_barLvl[b] > _barPk[b]) { _barPk[b] = _barLvl[b]; _barHold[b] = 6; }
            else if (_barHold[b] > 0) _barHold[b]--; else _barPk[b] = Math.Max(0, _barPk[b] - 0.03);
            int hh = (int)(_barLvl[b] * ph);
            int bx = x0 + b * step;
            for (int r = 0; r < hh; r++)
            {
                double ratio = (double)r / ph; byte c = ratio < 0.4 ? D2 : ratio < 0.75 ? D3 : ratio < 0.9 ? D4 : D5;
                for (int w = 0; w < bw; w++) cv.Put(bx + w, y0 + ph - 1 - r, '\u2588', c);
            }
            int pr = (int)(_barPk[b] * ph);
            if (pr > hh && pr < ph) for (int w = 0; w < bw; w++) cv.Put(bx + w, y0 + ph - 1 - pr, '\u2500', Wh);
        }
        for (int x = 0; x < CW; x++) cv.Put(x0 + x, y0 + ph, '\u2500', D1);
        return ("AUDIO", "48kHz 24BIT");
    }

    private static (string, string) DrawScan(Canvas cv, int x0, int y0, int CW, int B)
    {
        int nb = 16; foreach (int cand in new[] { 16, 12, 8, 4 }) { if (8 + 2 + cand * 3 + 1 + cand <= CW) { nb = cand; break; } nb = cand; }
        if (8 + 2 + nb * 3 + 1 + nb > CW) { nb = 4; while (nb > 1 && 8 + 2 + nb * 3 > CW) nb--; }
        cv.Text(x0, y0, "OFFSET", D5); for (int x = 0; x < CW; x++) cv.SetBg(x0 + x, y0, D0);
        int rows = B - 1; if (rows < 1) rows = 1;
        if (_frame % 2 == 0) _scanOff += nb;
        int head = _frame % rows;
        for (int r = 0; r < rows; r++)
        {
            int ry = y0 + 1 + r; int off = _scanOff + r * nb;
            cv.Text(x0, ry, "0x" + (off & 0xFFFFFF).ToString("X6"), G2);
            int hx = x0 + 9;
            for (int b = 0; b < nb; b++)
            {
                int val = Rng.Next(10) < 2 ? 0 : Rng.Next(256);
                byte c = val == 0 ? D0 : (val >= 0x20 && val <= 0x7E) ? D3 : D2;
                cv.Text(hx + b * 3, ry, val.ToString("X2"), c);
                int gx = x0 + 9 + nb * 3 + 1 + b;
                if (gx < x0 + CW) cv.Put(gx, ry, (val >= 0x20 && val <= 0x7E) ? (char)val : '.', val >= 0x20 && val <= 0x7E ? G2 : D0);
            }
            if (r == head) { cv.Put(x0 - 1 >= 0 ? x0 - 1 : x0, ry, '>', D5); for (int x = 0; x < CW; x++) { cv.SetBg(x0 + x, ry, D0); cv.SetFg(x0 + x, ry, Lift(cv.Fg(x0 + x, ry))); } }
        }
        int pct = (int)((_scanOff & 0xFFFFF) / (double)0x100000 * 100);
        return ("SCAN", pct + "%");
    }

    private static (string, string) DrawGrid(Canvas cv, int x0, int y0, int CW, int B, int heat)
    {
        int cols = (CW + 1) / 5, rows = B; int cells = Math.Max(1, cols * rows);
        if (_gridVal.Length != cells) { _gridVal = new int[cells]; _gridAge = new byte[cells]; for (int i = 0; i < cells; i++) _gridVal[i] = Rng.Next(0x10000); }
        int m = 1 + cells / 40 + heat * cells / 200;
        for (int k = 0; k < m; k++) { int i = Rng.Next(cells); _gridVal[i] = Rng.Next(10) < 2 ? 0 : Rng.Next(0x10000); _gridAge[i] = 0; }
        for (int i = 0; i < cells; i++) if (_gridAge[i] < 255) _gridAge[i]++;
        int scanCol = (_frame / 2) % Math.Max(1, cols);
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
            {
                int i = r * cols + c; if (i >= cells) continue; int v = _gridVal[i]; byte age = _gridAge[i];
                byte col = v == 0 ? D0 : age == 0 ? Wh : age == 1 ? D5 : age <= 3 ? D4 : age <= 8 ? D3 : D2;
                if (c == scanCol) col = Lift(col);
                cv.Text(x0 + c * 5, y0 + r, v.ToString("X4"), col);
            }
        return ("NUMBERS", "MUT " + (m * 12) + "/s");
    }

    private static (string, string) DrawCore(Canvas cv, int x0, int y0, int CW, int B, string[] tok)
    {
        int temp = tok.Length > 2 && int.TryParse(tok[2], out int t) ? t : 40;
        int cool = tok.Length > 3 && int.TryParse(tok[3], out int cc) ? cc : 50;
        int react = tok.Length > 4 && int.TryParse(tok[4], out int rr) ? rr : 50;
        temp = Math.Clamp(temp, 0, 100); cool = Math.Clamp(cool, 0, 100); react = Math.Clamp(react, 0, 100);
        int readoutRows = B < 10 ? 1 : 2;
        int orbRows = B - readoutRows; if (orbRows < 1) orbRows = 1;
        double heat = temp / 100.0;
        double cx = x0 + CW / 2.0, cy = y0 + orbRows / 2.0;
        double rMax = Math.Min((orbRows - 1) / 2.0, CW / 4.0);
        double omega = 0.12 + 0.5 * heat, pulse = 0.5 + 0.5 * Math.Sin(_frame * omega), amp = 0.04 + 0.12 * heat;
        double rc = rMax * (0.25 + 0.40 * heat) * (1 - amp + amp * pulse);
        if (rc < 0.5) rc = 0.5;
        for (int y = 0; y < orbRows; y++)
            for (int x = 0; x < CW; x++)
            {
                double dx = (x0 + x - cx) * 0.5, dy = y0 + y - cy, d = Math.Sqrt(dx * dx + dy * dy);
                double q = d / rc;
                if (q < 0.35) cv.Put(x0 + x, y0 + y, '\u2588', heat >= 0.8 ? Wh : D5);
                else if (q < 0.70) cv.Put(x0 + x, y0 + y, '\u2588', D4);
                else if (q < 1.00) cv.Put(x0 + x, y0 + y, '\u2592', D3);
                else if (q < 1.25) cv.Put(x0 + x, y0 + y, '\u2591', D2);
                else if (q < 1.45 && heat >= 0.5) cv.Put(x0 + x, y0 + y, '\u2591', D1);
            }
        int ry = y0 + orbRows, x1 = x0 + CW - 1;
        DrawFieldRow(cv, x0, x1, ry, new Field { Label = "TEMP", Val = temp + "C", Cur = temp, Max = 100, Tone = "heat" });
        if (readoutRows == 2 && ry + 1 <= y0 + B - 1)
            DrawFieldRow(cv, x0, x1, ry + 1, new Field { Label = "COOL", Val = cool + "%", Cur = cool, Max = 100, Tone = "cool" });
        string state = temp < 50 ? "NOMINAL" : temp < 80 ? "RISING" : "CRITICAL";
        return ("CORE", "T " + temp + "C " + state);
    }

    // ---------------- glitch ----------------
    private static readonly string[] Errs = { "SIGNAL LOST", "NO CARRIER", "0xDEAD", "CRC FAIL", "//FAULT", "SEGV", "PARITY ERR", "LINK DOWN" };

    private static void GlitchOverlay(Canvas cv, int C, int R, string slot, ref string label)
    {
        double I = 0.7;
        if (_glUntil > _glStart) { double t = Math.Clamp((double)(DateTime.UtcNow.Ticks - _glStart) / (_glUntil - _glStart), 0, 1); I = t < 0.15 ? 1.0 : (1 - t) * 0.9 + Rng.NextDouble() * 0.1; }
        int bands = 1 + (int)(3 * I);
        for (int bnd = 0; bnd < bands; bnd++)
        {
            int by = 1 + Rng.Next(Math.Max(1, R - 2)); int dx = (Rng.Next(13) - 6);
            if (dx == 0) dx = 1;
            var rowCh = new char[C]; var rowFg = new byte[C];
            for (int x = 0; x < C; x++) { rowCh[x] = cv.Ch(x, by); rowFg[x] = cv.Fg(x, by); }
            for (int x = 0; x < C; x++) { int sx = x - dx; cv.Put(x, by, sx >= 0 && sx < C ? rowCh[sx] : '\u2591', sx >= 0 && sx < C ? rowFg[sx] : D1); }
        }
        // RGB split on a couple rows
        int splits = I > 0.7 ? R : 2; int o = I > 0.6 ? 2 : 1;
        for (int s = 0; s < splits; s++)
        {
            int ry = 1 + Rng.Next(Math.Max(1, R - 2));
            var orig = new char[C]; for (int x = 0; x < C; x++) orig[x] = cv.Ch(x, ry);
            for (int x = 0; x < C; x++)
            {
                if (orig[x] != ' ') cv.Put(x, ry, orig[x], Wh);
                else if (x - o >= 0 && orig[x - o] != ' ') cv.Put(x, ry, orig[x - o], D4);
                else if (x + o < C && orig[x + o] != ' ') cv.Put(x, ry, orig[x + o], Cy3);
            }
        }
        // corruption
        for (int y = 1; y <= R - 2; y++)
            for (int x = 1; x < C - 1; x++)
                if (Rng.NextDouble() < 0.01 + 0.12 * I)
                {
                    string pool = "#%&$@*+=?<>/\\|01";
                    char ch = Rng.Next(3) == 0 ? "\u2588\u2592\u2591"[Rng.Next(3)] : pool[Rng.Next(pool.Length)];
                    byte[] cols = { D2, D3, D4, Wh, Cy2 };
                    cv.Put(x, y, ch, cols[Rng.Next(cols.Length)]);
                }
        // error stamps
        if (I > 0.3)
        {
            int n = 1 + Rng.Next(2);
            for (int k = 0; k < n; k++)
            {
                string e = Errs[Rng.Next(Errs.Length)];
                int ex = 2 + Rng.Next(Math.Max(1, C - e.Length - 3)), ey = 1 + Rng.Next(Math.Max(1, R - 2));
                for (int i = 0; i < e.Length; i++) cv.Put(ex + i, ey, e[i], Ink, D4, true);
            }
        }
        // bar swap
        if ((_frame / 4) % 2 == 0) { for (int x = 0; x < C; x++) cv.Put(x, 0, ' ', Ink, D3); cv.Text(1, 0, "NO CARRIER", Ink, D3, true); }
        cv.Put(C - 4, 0, '[', Wh, D4, true); cv.Put(C - 3, 0, '!', Wh, D4, true); cv.Put(C - 2, 0, ']', Wh, D4, true);
        // center box
        if (I > 0.45 && C >= 24 && R >= 8)
        {
            int bwid = 20, bx = (C - bwid) / 2, byv = R / 2 - 1;
            for (int x = 0; x < bwid; x++) { cv.Put(bx + x, byv, '\u2500', D4); cv.Put(bx + x, byv + 2, '\u2500', D4); }
            cv.Put(bx, byv, '\u250c', D4); cv.Put(bx + bwid - 1, byv, '\u2510', D4);
            cv.Put(bx, byv + 2, '\u2514', D4); cv.Put(bx + bwid - 1, byv + 2, '\u2518', D4);
            for (int x = 1; x < bwid - 1; x++) cv.Put(bx + x, byv + 1, ' ', Wh, 0);
            string msg = (_frame / 6) % 2 == 0 ? "SIGNAL  LOST" : "NO CARRIER";
            cv.Text(bx + (bwid - msg.Length) / 2, byv + 1, msg, Wh, 0, true);
        }
    }

    // ---------------- ascii art ----------------
    private static readonly Dictionary<string, string[]> Art = new()
    {
        ["worm"] = new[] { "   .-~~~~~~-.", " .'  ______  '.", "/  .'VVVVVV'.  \\", "| /  >    <  \\ |", "| \\  >    <  / |", " \\  '.^^^^^^.' /", "  '.        .'", "    '-.____.-'" },
        ["rootkit"] = new[] { " .---------------.", " |  .---.  .---. |", " |  | o |  | o | |", " |  '---'^^'---' |", " |    \\_/VV\\_/   |", " '--+--+--+--+--'" },
        ["trojan"] = new[] { "   ,;,;,;,;,;", "  .'::::::::'.", " /  ,------.  \\", "|  / \\    / \\ |", "| | (o)  (o) | |", "| |    ||    | |", " '-._  ||  _.-'" },
        ["daemon"] = new[] { " (\\            /)", "  \\\\  ______  //", "   \\\\.'    './/", "   \\/ \\  / \\/", "   | (o)(o) |", "   | .____. |", "    \\ \\/\\/ /", "     '.__.'" },
        ["wraith"] = new[] { "   .-''''-.", "  .' .--. '.", " /  /    \\  \\", "|  | o  o |  |", "|  |      |  |", "|   \\ ~~ /   |", " \\   '--'   /", " /\\/\\/\\/\\/\\" },
        ["generic"] = new[] { "    _______", "  .'       '.", " /  _     _  \\", "|  (o)   (o)  |", "|      ^      |", " \\  .'VVV'.  /", "  '.|_|_|_|.'", "    '-----'" },
        ["ransom"] = new[] { "    .-----.", "   / .---. \\", "   | |   | |", " .-'-'---'-'-.", " |   .---.   |", " |   | $ |   |", " '-----------'" },
        ["botnet"] = new[] { " o---o   o---o", "  \\   \\ /   /", "   o-( @ )-o", "  /   / \\   \\", " o---o   o---o" },
        ["keylog"] = new[] { "    .--------.", "   (  ( @@ )  )", "    '--------'", " .--------------.", " |[Q][W][E][R]|", " '--------------'" },
        ["me"] = new[] { "   .-----.", "  / .---. \\", " | / o o \\ |", " | |  -  | |", " |  \\___/  |", ".'---| |---'.", "/     |     \\" },
    };

    // ---------------- helpers ----------------
    private static string Pad(string s, int w) => s.Length >= w ? s[..w] : s.PadRight(w);
    private static string Pad3(int v) { string s = Math.Clamp(v, 0, 999).ToString(CultureInfo.InvariantCulture); return s.PadLeft(3); }

    private static List<string>? TryRead(string path)
    {
        try { if (!File.Exists(path)) return null; return File.ReadAllText(path).Replace("\r", "").Split('\n').ToList(); }
        catch { return null; }
    }

    private static bool GlitchActive(string dir)
    {
        if (dir.Length == 0) return false;
        try
        {
            string f = Path.Combine(dir, "glitch.txt");
            if (File.Exists(f) && long.TryParse(File.ReadAllText(f).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long until))
            {
                if (until != _glUntil) { _glUntil = until; _glStart = DateTime.UtcNow.Ticks; }
                return DateTime.UtcNow.Ticks < until;
            }
        }
        catch { }
        return false;
    }

    private static bool ParentAlive(int pid)
    {
        try { return !Process.GetProcessById(pid).HasExited; }
        catch { return false; }
    }

    private static int SafeW() { try { return Math.Clamp(Console.WindowWidth, 10, 200); } catch { return 40; } }
    private static int SafeH() { try { return Math.Clamp(Console.WindowHeight, 6, 60); } catch { return 20; } }

    [SupportedOSPlatform("windows")]
    private static void PositionSelf(string slot)
    {
        IntPtr hwnd = GetConsoleWindow();
        if (hwnd == IntPtr.Zero) return;
        int sw = GetSystemMetrics(0), sh = GetSystemMetrics(1);
        if (sw <= 0 || sh <= 0) return;
        int x, y, ww, hh;
        switch (slot)
        {
            case "tr": x = sw * 2 / 3; y = 0; ww = sw / 3; hh = sh / 2; break;
            case "bl": x = 0; y = sh / 2; ww = sw / 3; hh = sh / 2; break;
            default: x = sw * 2 / 3; y = sh / 2; ww = sw / 3; hh = sh / 2; break;
        }
        MoveWindow(hwnd, x, y, ww, hh, true);
    }

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();
    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
}
