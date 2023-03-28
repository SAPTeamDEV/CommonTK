using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace SAPTeam.CommonTK.Console
{
    /// <summary>
    /// Represent methods for control Application consoles.
    /// </summary>
    public static class ConsoleManager
    {
        private const string Kernel32_DllName = "kernel32.dll";
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int MF_BYCOMMAND = 0x00000000;
        private const int SC_CLOSE = 0xF060;

        [DllImport(Kernel32_DllName)] private static extern bool AllocConsole();
        [DllImport(Kernel32_DllName)] private static extern bool FreeConsole();
        [DllImport(Kernel32_DllName)] private static extern IntPtr GetConsoleWindow();
        [DllImport(Kernel32_DllName)] private static extern int GetConsoleOutputCP();
        [DllImport(Kernel32_DllName)] private static extern bool AttachConsole(int dwProcessId);

        [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")] private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
        [DllImport("user32.dll")] private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        private static void DisableCloseButton()
        {
            _ = DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
        }

        /// <summary>
        /// Gets or Sets Type of current Application's console Text rendering.
        /// </summary>
        public static ConsoleType Type { get; set; }

        /// <summary>
        /// Gets console Launching method.
        /// </summary>
        public static ConsoleLaunchMode Mode { get; private set; }

        /// <summary>
        /// Checks if The Application has Console.
        /// </summary>
        public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;

        /// <summary>
        /// Shows up existing Console Window, if Console Window not found then creates a new Console.
        /// </summary>
        /// <param name="mode">
        /// Determines method that used for launching a new Console. if there is an existing Console, this value is ignored.
        /// </param>
        /// <param name="canClose">
        /// Determines console window can be closed without handling or Need to be closed in expected way.
        /// </param>
        public static void ShowConsole(ConsoleLaunchMode mode, bool canClose = false)
        {
            if (!HasConsole)
            {
                switch (mode)
                {
                    case ConsoleLaunchMode.Allocation:
                        AllocateConsole();
                        break;
                    case ConsoleLaunchMode.AttachParent:
                        AttachToParent();
                        break;
                    case ConsoleLaunchMode.AttachProcess:
                        AttachProcess(CreateConsole());
                        break;
                }
                Mode = mode;

                System.Console.Title = AppDomain.CurrentDomain.FriendlyName;

                if (!canClose)
                {
                    DisableCloseButton();
                }
            }
            else
            {
                ShowWindow(GetConsoleWindow(), SW_SHOW);
                Interact.BringToFront(GetConsoleWindow());
            }
        }

        /// <summary>
        /// Hides or Releases Application Console.
        /// </summary>
        /// <param name="release">
        /// Determines that current Console should be closed or just hide it.
        /// </param>
        public static void HideConsole(bool release = true)
        {
            if (release)
            {
                ReleaseConsole();
            }
            else
            {
                ShowWindow(GetConsoleWindow(), SW_HIDE);
            }
        }

        /// <summary>
        /// Creates a new console instance if the process is not attached to a console already.
        /// </summary>
        private static void AllocateConsole()
        {
            if (!HasConsole)
            {
                AllocConsole();
                InvalidateOutAndError();
            }
        }

        /// <summary> 
        /// If the process has a console attached to it, it will be detached and no longer visible. Writing to the System.Console is still possible, but no output will be shown.
        /// </summary>
        private static void ReleaseConsole()
        {
            if (HasConsole)
            {
                SetOutAndErrorNull();
                FreeConsole();
            }
        }

        /// <summary>
        /// Takes control of the Caller's Console.
        /// </summary>
        private static void AttachToParent()
        {
            if (!HasConsole)
            {
                AttachConsole(-1);
            }
        }

        /// <summary>
        /// Creates a new Console process.
        /// </summary>
        /// <returns><see cref="Process"/> object of new Console.</returns>
        private static Process CreateConsole()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe"
            };
            Process con = Process.Start(startInfo);
            return con;
        }

        /// <summary>
        /// Attaches to given <see cref="Process"/> Console and kills that process.
        /// </summary>
        /// <param name="process">A Process that have Console.</param>
        private static void AttachProcess(Process process)
        {
            Thread.Sleep(1000);
            Type = ConsoleType.Native;
            AttachConsole(process.Id);
            process.Kill();
            System.Console.Clear();
        }

        private static void ForceSet(ConsoleField field, object obj)
        {
            Type type = typeof(System.Console);
            string pField = "";
            switch (field)
            {
                case ConsoleField.In:
                    pField = "s_in";
                    break;
                case ConsoleField.Out:
                    pField = "s_out";
                    break;
                case ConsoleField.Error:
                    pField = "s_error";
                    break;
            }
            System.Reflection.FieldInfo fControl = type.GetField(pField, BindingFlags.Static | BindingFlags.NonPublic);
            Debug.Assert(fControl != null);
            fControl.SetValue(obj, obj);
        }

        private static void InvalidateOutAndError()
        {
            ForceSet(ConsoleField.Out, null);
            ForceSet(ConsoleField.Error, null);
        }

        private static void SetOutAndErrorNull()
        {
            System.Console.SetOut(TextWriter.Null);
            System.Console.SetError(TextWriter.Null);
        }
    }
}