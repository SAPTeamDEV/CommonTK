namespace SAPTeam.CommonTK.Console;

public struct ColorSet
{
    private static ColorSet colors = new();

    public readonly ConsoleColor Back;
    public readonly ConsoleColor Fore;

    public static ColorSet ScreenMesage => new(ConsoleColor.White, ConsoleColor.Black);
    public static ColorSet InvertedScreenMesage => new(ConsoleColor.Black, ConsoleColor.White);

    internal static ColorSet Current { get => colors; set { colors = value; ResetColor(); } }

    public ColorSet()
    {
        Back = ConsoleColor.Black;
        Fore = ConsoleColor.Gray;
    }

    public ColorSet(ConsoleColor back, ConsoleColor fore)
    {
        Back = back;
        Fore = fore;
    }

    public void ChangeColor()
    {
        System.Console.BackgroundColor = Back;
        System.Console.ForegroundColor = Fore;
    }
}
