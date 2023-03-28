using System;
#if NET5_0 && WINDOWS
using System.Windows.Forms;
#endif

using SAPTeam.CommonTK.Contexts;

namespace SAPTeam.CommonTK.Console
{
    /// <summary>
    /// Provides various functions that is not found in <see cref="Console"/> .NET Class.
    /// </summary>
    public static partial class Utils
    {
        /// <summary>
        /// Shows <paramref name="text"/> Message to Console or MessageBox Depending on the environment.
        /// </summary>
        /// <param name="text">Message Text.</param>
        /// <param name="newLine">Insert Line Terminator at the end of text. (For Console)</param>
        /// <exception cref="InterfacrNotImplementedException"></exception>
        public static void Echo(string text = "", bool newLine = true)
        {
            if (text == null)
            {
                text = "";
            }

            switch (Context.Interface)
            {
                case InteractInterface.Console:
                    {
                        if (text.Length > 0 && Context.Current.HasContext<DisposableWriter>() && !Context.Current.HasContext<RedirectConsole>())
                        {
                            Context.Current.GetContext<DisposableWriter>().AddCoords(System.Console.CursorTop, System.Console.CursorLeft + text.Length);
                        }

                        if (newLine)
                        {
                            System.Console.WriteLine(text);
                            if (Context.Current.HasContext<RedirectConsole>())
                            {
                                Context.Current.GetContext<RedirectConsole>().Line++;
                            }
                        }
                        else
                        {
                            System.Console.Write(text);
                        }
                    }
                    break;

#if NET5_0 && WINDOWS
                case InteractInterface.UI:
                        MessageBox.Show(text, AppDomain.CurrentDomain.FriendlyName);
                        break;
#endif

                default:
                        throw new InvalidOperationException();
            }
        }

        public static void Echo(Colorize colorizedString, bool newLine = true)
        {
            if (Context.Interface == InteractInterface.Console)
            {
                foreach (var (text, color) in colorizedString.ColorizedString)
                {
                    if (color != null)
                    {
                        System.Console.ForegroundColor = (ConsoleColor)color;
                    }
                    Echo(text, false);
                    ResetColor();
                }
                Echo(newLine: newLine);
            }
            else
            {
                Echo(colorizedString.Text);
            }
        }

        public static int GetLine()
        {
            return Context.Current.HasContext<RedirectConsole>() ? Context.Current.GetContext<RedirectConsole>().Line + System.Console.CursorTop : System.Console.CursorTop;
        }

        /// <summary>
        /// Clears Previous or Current line contents and set cursor to Beginning of that line.
        /// </summary>
        /// <param name="inLineClear">Clear this Line.</param>
        public static void ClearLine(bool inLineClear = false, int? length = null)
        {
            System.Console.SetCursorPosition(0, System.Console.CursorTop - (inLineClear ? 0 : 1));
            System.Console.Write(new string(' ', (int)(length == null ? System.Console.BufferWidth : length)));
            System.Console.SetCursorPosition(0, Math.Max(0, System.Console.CursorTop - (ConsoleManager.Type == ConsoleType.Native ? 0 : 1)));
        }

        internal static void ClearInLine()
        {
            ClearLine(true);
        }
    }
}