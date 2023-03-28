using System;
using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK.Console
{
    public static partial class Utils
    {
        public static void SetCursor(int left, int top, int spacing)
        {
            if (top - spacing < System.Console.WindowTop && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                System.Console.WindowTop = Math.Max(0, System.Console.WindowTop - 1);
            }
            System.Console.SetCursorPosition(left, top);
        }

        public static void SetColor(ColorSet colors)
        {
            System.Console.BackgroundColor = colors.Back;
            System.Console.ForegroundColor = colors.Fore;
        }

        public static void ResetColor()
        {
            System.Console.BackgroundColor = ColorSet.Current.Back;
            System.Console.ForegroundColor = ColorSet.Current.Fore;
        }

        public static int GetCenterPosition(int textLength)
        {
            return System.Console.BufferWidth / 2 - textLength / 2;
        }
    }
}