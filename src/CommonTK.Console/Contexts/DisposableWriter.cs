using SAPTeam.CommonTK.Console;

namespace SAPTeam.CommonTK.Contexts
{
    public class DisposableWriter : Context
    {
        private readonly List<ConsoleCoords> coords = new();
        private readonly List<int> lines = new();
        private bool lineClear;
        private ConsoleColor backColor;
        private ConsoleColor foreColor;

        public DisposableWriter(bool lineClear = false, ConsoleColor backgroundColor = ConsoleColor.Black, ConsoleColor foregroundColor = ConsoleColor.Gray) : base(lineClear, backgroundColor, foregroundColor)
        {

        }

        protected override void CreateContext()
        {
            ColorSet.Current = new(backColor, foreColor);
        }

        protected override void DisposeContext()
        {
            ColorSet.Current = new();

            Clear();

            coords.Clear();
        }

        public void Clear()
        {
            foreach (var coord in coords)
            {
                System.Console.CursorTop = coord.X;
                if (lineClear)
                {
                    ClearLine(true, null);
                }
                else
                {
                    ClearLine(true, coord.Length);
                }
            }

            if (lines.Count > 0)
            {
                System.Console.CursorTop = lines.Min();
            }
        }

        public void AddCoords(int x, int length)
        {
            if (!lines.Contains(x))
            {
                lines.Add(x);
                coords.Add(new() { Length = length, X = x });
            }
        }

        protected override void ArgsHandler(dynamic[] args)
        {
            lineClear = args[0];
            backColor = args[1];
            foreColor = args[2];
        }
    }
}