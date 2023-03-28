using System;
using System.Collections.Generic;
using System.Linq;

using SAPTeam.CommonTK.Console;

namespace SAPTeam.CommonTK.Contexts
{
    public class DisposableWriter : Context
    {
        private readonly List<ConsoleCoords> coords = new List<ConsoleCoords>();
        private readonly List<int> lines = new List<int>();
        private bool lineClear;
        private ConsoleColor backColor;
        private ConsoleColor foreColor;

        public DisposableWriter(bool lineClear = false, ConsoleColor backgroundColor = ConsoleColor.Black, ConsoleColor foregroundColor = ConsoleColor.Gray) : base(lineClear, backgroundColor, foregroundColor)
        {

        }

        protected override void CreateContext()
        {
            ColorSet.Current = new ColorSet(backColor, foreColor);
        }

        protected override void DisposeContext()
        {
            ColorSet.Current = new ColorSet();

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
                    Utils.ClearLine(true, null);
                }
                else
                {
                    Utils.ClearLine(true, coord.Length);
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
                coords.Add(new ConsoleCoords() { Length = length, X = x });
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