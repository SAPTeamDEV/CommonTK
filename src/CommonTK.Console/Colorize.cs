using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if NET461
using IEnumerable.Append;
#endif

namespace SAPTeam.CommonTK.Console
{
    public struct Colorize
    {
        public string Text;
        private readonly ConsoleColor[] colors;
        private readonly string clearText;

        public IEnumerable<(string text, ConsoleColor? color)> ColorizedString { get; }

        public Colorize(string text, params ConsoleColor[] colors)
        {
            Text = text;
            this.colors = colors;
            clearText = null;

            var pieces = Regex.Split(text, @"(\[[^\]]*\])");
            ColorizedString = new (string text, ConsoleColor? color)[pieces.Length];

            int ci = 0;

            for (int i = 0; i < pieces.Length; i++)
            {
                string piece = pieces[i];
                bool formatted = false;

                if (piece.StartsWith("[") && piece.EndsWith("]"))
                {
                    formatted = true;
                    piece = piece.Substring(1, piece.Length - 1);
                }

                if (formatted && colors.Length > 0)
                {
                    ColorizedString = ColorizedString.Append((piece, colors[Math.Min(ci, colors.Length - 1)])) as IEnumerable<(string, ConsoleColor?)>;
                    ci++;
                }
                else
                {
                    ColorizedString = (IEnumerable<(string, ConsoleColor?)>)ColorizedString.Append((piece, null));
                }
            }
        }

        public override string ToString()
        {
            return this;
        }

        public static Colorize operator +(Colorize x, string y)
        {
            return new Colorize(x.Text + y, x.colors);
        }

        public static Colorize operator +(Colorize x, Colorize y)
        {
            ConsoleColor[] colors = new ConsoleColor[x.colors.Length + y.colors.Length];
            int i = 0;
            foreach (var color in x.colors.Concat(y.colors))
            {
                colors[i] = color;
                i++;
            }
            return new Colorize(x.Text + y.Text, colors);
        }

        public static implicit operator string(Colorize x)
        {
            string clearedText = "";

            if (x.clearText == null)
            {
                foreach (var (text, color) in x.ColorizedString)
                {
                    clearedText += text;
                }
            }
            return clearedText;
        }
    }
}