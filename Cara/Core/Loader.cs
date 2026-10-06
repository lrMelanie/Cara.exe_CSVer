namespace Cara.Core;

/// Startup loading bar and the ASCII logotype (ported 1:1 from load.cpp).
public static class Loader
{
    private const string LoadingChars = "#-";
    private const int LoadingWidth = 50;

    public static void ShowLoading()
    {
        Console.Clear();
        Console.WriteLine("SYSTEM INITIALIZATION");
        Console.Write("[");
        for (int i = 0; i < LoadingWidth; i++)
        {
            Console.Write(LoadingChars[i % LoadingChars.Length]);
            Thread.Sleep(50 + i * 15);
        }
        Console.WriteLine("]");
    }

    public static void ShowLogotype()
    {
        const string grn = "\x1b[38;5;46m";
        const string rst = "\x1b[0m";
        Console.WriteLine(grn +
            "----------------------------------\n" +
            "  _____          _____             \n" +
            " / ____|   /\\   |  __ \\     /\\     \n" +
            "| |       /  \\  | |__) |   /  \\    \n" +
            "| |      / /\\ \\ |  _  /   / /\\ \\   \n" +
            "| |____ / ____ \\| | \\ \\  / ____ \\  \n" +
            " \\_____/_/    \\_\\_|  \\_\\/_/    \\_\\ \n" +
            "----------------------------------" + rst);
    }
}
