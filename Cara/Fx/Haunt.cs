using System.Diagnostics;

namespace Cara.Fx;

/// Atmospheric "haunting": launches the stray .bat effects from
/// resources/Tools/ - as hidden codes, rare minigame jumpscares, or a very
/// rare ambient trigger. All of them are harmless window/visual gags.
public static class Haunt
{
    private static readonly string ToolsDir =
        Path.Combine(AppContext.BaseDirectory, "resources", "Tools");

    private static readonly string[] Bats =
    {
        "BlockchainIntegrity.bat",
        "CCD.bat",
        "Surveillance Hub.bat",
        "ac_v2.3.bat",
        "donttouch.bat",
        "evprecorder.bat",
        "firewall.bat",
        "sec.bat",
        "signal_decay_monitor.bat",
        "static.bat"
    };

    // Hidden command -> effect file.
    private static readonly Dictionary<string, string> Codes = new(StringComparer.Ordinal)
    {
        { "WATCHING-YOU",       "Surveillance Hub.bat" },
        { "D34D-S1GN4L",        "signal_decay_monitor.bat" },
        { "EYES-IN-THE-STATIC", "static.bat" },
        { "EVP-REC",            "evprecorder.bat" },
        { "0V3RW4TCH",          "donttouch.bat" },
        { "D3CRYPT",            "CCD.bat" },
        { "F1R3W4LL",           "firewall.bat" },
        { "BL0CKCH41N",         "BlockchainIntegrity.bat" },
        { "R3C0NN3CT",          "ac_v2.3.bat" },
        { "ECH0-CH4MB3R",       "sec.bat" }
    };

    private static readonly Random Rng = new();

    public static void Launch(string batName)
    {
        try
        {
            string path = Path.Combine(ToolsDir, batName);
            if (!File.Exists(path)) return;
            Process.Start(new ProcessStartInfo("cmd.exe", $"/c start \"\" \"{path}\"") { UseShellExecute = false });
        }
        catch { }
    }

    public static void LaunchRandom()
    {
        if (Bats.Length == 0) return;
        Launch(Bats[Rng.Next(Bats.Length)]);
    }

    // 1-in-oneIn chance to fire a random effect. Used for jumpscares / ambient.
    public static void Maybe(int oneIn)
    {
        if (oneIn > 0 && Rng.Next(oneIn) == 0) LaunchRandom();
    }

    // Returns true if the input was a hidden effect code (and fired it).
    public static bool TryCode(string input)
    {
        if (Codes.TryGetValue(input, out string? bat)) { Launch(bat); return true; }
        return false;
    }
}
