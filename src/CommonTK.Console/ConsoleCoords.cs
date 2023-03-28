namespace SAPTeam.CommonTK.Console
{
    public struct ConsoleCoords
    {
        public static ConsoleCoords ScreenMessage => new ConsoleCoords()
        {
            X = 3,
            Position = ConsolePosition.Bottom,
            IsStatic = false,
            Center = true
        };

        public int X { get; set; }
        public int Y { get; set; }
        public int Length { get; set; }
        public ConsolePosition Position { get; set; }
        public bool IsStatic { get; set; }
        public bool Center { get; set; }

        public ConsoleCoords(int x, int y)
        {
            X = x;
            Y = y;

        Length = default;
        Position = default;
        IsStatic = true;
        Center = false;
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
}