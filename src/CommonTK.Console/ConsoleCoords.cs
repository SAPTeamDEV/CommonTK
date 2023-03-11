namespace SAPTeam.CommonTK.Console;

public struct ConsoleCoords
{
    public static ConsoleCoords ScreenMessage => new()
    {
        X = 3,
        Position = ConsolePosition.Bottom,
        IsStatic = false,
        Center = true
    };

    public int X { get; set; } = default;
    public int Y { get; set; } = default;
    public int Length { get; set; } = default;
    public ConsolePosition Position { get; set; } = default;
    public bool IsStatic { get; set; } = true;
    public bool Center { get; set; } = false;

    public ConsoleCoords()
    {

    }

    public void Focus()
    {
        System.Console.CursorTop = ResolveX();
        if (Y != default)
        {
            System.Console.CursorLeft = Y;
        }
    }

    public int ResolveX()
    {
        return IsStatic && Position == ConsolePosition.None
            ? X
            : Position == ConsolePosition.None || Position == ConsolePosition.Top
                ? System.Console.WindowTop + X
                : System.Console.WindowHeight - X + System.Console.WindowTop;
    }
}
