using System.Globalization;
using System.Text;
using static Cara.Ui.ConsoleUi;

namespace Cara.Ui;

/// Live multi-panel desktop (clock, calendar, countdown, numbers, run-log).
public static class Hud
{
    private const int PanelW = 36;   // one column
    private const int FullW = 74;    // both columns + gap
    private const int H = 8;         // content lines per top panel

    private static readonly string[] Services =
    {
        "Service Host: DHCP Client", "Service Host: DNS Client",
        "Service Host: Event Log", "Service Host: Local Service",
        "Service Host: Network List Service", "Service Host: Local System",
        "Application Frame Host", "System Settings Broker",
        "Session Manager", "Service Host: Network Connected Devices", "Not Found",
    };

    public static void Run()
    {
        EnableVt();
        Cls(); HideCur();
        var rng = new Random();
        try
        {
            while (true)
            {
                if (Console.KeyAvailable) { Console.ReadKey(true); break; }
                DateTime now = DateTime.Now;

                var sb = new StringBuilder("\x1b[H");
                sb.Append(Red + Bold + "  CARA.OS // DESKTOP" + Rst + Gry + "    "
                    + now.ToString("yyyy-MM-dd HH:mm:ss") + "    (any key to exit)" + Rst + "\x1b[K\n");
                sb.Append("\x1b[K\n");

                AppendTwo(sb, Panel("CLOCK", ClockBody(now), H, PanelW), Panel("CALENDAR", CalendarBody(now), H, PanelW));
                sb.Append("\x1b[K\n");
                AppendTwo(sb, Panel("COUNTDOWN", CountdownBody(now), H, PanelW), Panel("NUMBERS", NumbersBody(rng), H, PanelW));
                sb.Append("\x1b[K\n");
                foreach (var ln in Panel("RUN", RunBody(rng), 8, FullW))
                    sb.Append("  ").Append(ln).Append("\x1b[K\n");

                sb.Append("\x1b[J");
                Console.Write(sb.ToString());
                Thread.Sleep(250);
            }
        }
        catch { }
        ShowCur();
    }

    private static string[] Panel(string title, List<string> body, int height, int width)
    {
        int cw = width - 4;
        var lines = new List<string>();
        string t = title.Length > width - 6 ? title[..(width - 6)] : title;
        lines.Add(Red + "┌─ " + t + " " + new string('─', width - 5 - t.Length) + "┐" + Rst);
        for (int i = 0; i < height; i++)
        {
            string txt = i < body.Count ? body[i] : "";
            if (txt.Length > cw) txt = txt[..cw];
            lines.Add(Red + "│ " + Rst + Grn2 + txt.PadRight(cw) + Rst + Red + " │" + Rst);
        }
        lines.Add(Red + "└" + new string('─', width - 2) + "┘" + Rst);
        return lines.ToArray();
    }

    private static void AppendTwo(StringBuilder sb, string[] left, string[] right)
    {
        int h = Math.Max(left.Length, right.Length);
        string blank = new string(' ', PanelW);
        for (int i = 0; i < h; i++)
        {
            string l = i < left.Length ? left[i] : blank;
            string r = i < right.Length ? right[i] : blank;
            sb.Append("  ").Append(l).Append("  ").Append(r).Append("\x1b[K\n");
        }
    }

    private static List<string> ClockBody(DateTime now) => new()
    {
        "LOCAL   " + now.ToString("HH:mm:ss"),
        "UTC     " + now.ToUniversalTime().ToString("HH:mm:ss"),
        "DATE    " + now.ToString("yyyy-MM-dd"),
        "DAY     " + now.ToString("dddd", CultureInfo.InvariantCulture),
        "WEEK    " + ISOWeek.GetWeekOfYear(now),
    };

    private static List<string> CalendarBody(DateTime now)
    {
        var body = new List<string> { now.ToString("MMMM yyyy", CultureInfo.InvariantCulture), "Su Mo Tu We Th Fr Sa" };
        int days = DateTime.DaysInMonth(now.Year, now.Month);
        int lead = (int)new DateTime(now.Year, now.Month, 1).DayOfWeek;
        var cells = new List<string>();
        for (int i = 0; i < lead; i++) cells.Add("  ");
        for (int d = 1; d <= days; d++) cells.Add(d.ToString().PadLeft(2));
        while (cells.Count % 7 != 0) cells.Add("  ");
        for (int i = 0; i < cells.Count; i += 7)
            body.Add(string.Join(" ", cells.GetRange(i, 7)));
        return body;
    }

    private static List<string> CountdownBody(DateTime now)
    {
        TimeSpan eoy = new DateTime(now.Year, 12, 31, 23, 59, 59) - now;
        TimeSpan mid = now.Date.AddDays(1) - now;
        return new List<string>
        {
            "END OF YEAR",
            eoy.Days + "d " + eoy.Hours + "h " + eoy.Minutes + "m " + eoy.Seconds + "s",
            "",
            "NEXT MIDNIGHT",
            mid.Hours.ToString("00") + ":" + mid.Minutes.ToString("00") + ":" + mid.Seconds.ToString("00"),
        };
    }

    private static List<string> NumbersBody(Random rng)
    {
        var nb = new List<string>();
        for (int r = 0; r < 6; r++)
            nb.Add($"{rng.Next(10000000, 100000000)} {rng.Next(10000000, 100000000)} {rng.Next(10000000, 100000000)}");
        return nb;
    }

    private static List<string> RunBody(Random rng)
    {
        var lb = new List<string>();
        for (int i = 0; i < 8; i++)
        {
            string t = DateTime.Now.AddSeconds(-rng.Next(0, 90)).ToString("HH:mm:ss");
            lb.Add(t + "   " + rng.Next(100000, 1000000) + "   " + Services[rng.Next(Services.Length)]);
        }
        return lb;
    }
}
