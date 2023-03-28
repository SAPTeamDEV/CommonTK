namespace SAPTeam.CommonTK.Console
{
    public struct ColorSet
    {
        private static ColorSet colors = new();

        public readonly ConsoleColor Back;
        public readonly ConsoleColor Fore;

        public static ColorSet ScreenMesage => new(ConsoleColor.White, ConsoleColor.Black);
        public static ColorSet InvertedScreenMesage => new(ConsoleColor.Black, ConsoleColor.White);

        internal static ColorSet Current { get => colors; set { colors = value; Utils.ResetColor(); } }

        public ColorSet(ConsoleColor back = ConsoleColor.Black, ConsoleColor fore = ConsoleColor.Gray)
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
}