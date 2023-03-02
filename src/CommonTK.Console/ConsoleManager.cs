using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK.Console;

/// <summary>
/// Represent methods for control Application consoles.
/// </summary>
public class ConsoleManager
{
    private const string kernel32_DllName = "kernel32.dll";
    public const int SW_HIDE = 0;
    public const int SW_SHOW = 5;
    public const int MF_BYCOMMAND = 0x00000000;
    public const int SC_CLOSE = 0xF060;

    [DllImport(kernel32_DllName)] private static extern bool AllocConsole();
    [DllImport(kernel32_DllName)] private static extern bool FreeConsole();
    [DllImport(kernel32_DllName)] private static extern IntPtr GetConsoleWindow();
    [DllImport(kernel32_DllName)] private static extern int GetConsoleOutputCP();
    [DllImport(kernel32_DllName)] private static extern bool AttachConsole(int dwProcessId);

    [DllImport("user32.dll")] private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    [DllImport("user32.dll")] private static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);
    [DllImport("user32.dll")] private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

    private static void DisableCloseButton()
    {
        _ = DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_CLOSE, MF_BYCOMMAND);
    }

    /// <summary>
    /// Checks if The Application has Console.
    /// </summary>
    public static bool HasConsole => GetConsoleWindow() != IntPtr.Zero;

    /// <summary>
    /// Creates a new console instance if the process is not attached to a console already.
    /// </summary>
    private static void Show()
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
    private static void Hide()
    {
        if (HasConsole)
        {
            SetOutAndErrorNull();
            FreeConsole();
        }
    }

    public static void Toggle()
    {
        if (HasConsole)
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    /// <summary>
    /// Releases Application Console.
    /// </summary>
    public static void HideConsole()
    {
        Hide();
    }

    /// <summary>
    /// Shows up existing Console Window, if Console Window not found then creates a new Console.
    /// </summary>
    public static void ShowConsole()
    {
        if (!HasConsole)
        {
            Show();
            System.Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
            DisableCloseButton();
            // AllocConsole();
            // AttachProcess(CreateConsole());
        }
        else
        {
            Interact.BringToFront(GetConsoleWindow());
            // ShowWindow(GetConsoleWindow(), SW_SHOW);
        }
    }

    /// <summary>
    /// Takes control of the Caller's Console.
    /// </summary>
    public static void AttachToParent()
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
    public static Process CreateConsole()
    {
        ProcessStartInfo startInfo = new()
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
    public static void AttachProcess(Process process)
    {
        Thread.Sleep(1000);
        // Info.Application = ApplicationType.Console;
        AttachConsole(process.Id);
        process.Kill();
        System.Console.Clear();
    }

    public static void ForceSet(ConsoleField field, object? obj)
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
        System.Reflection.FieldInfo fControl = type.GetField(pField, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Debug.Assert(fControl != null);
        fControl.SetValue(obj, obj);
    }

    private static void InvalidateOutAndError()
    {
        ForceSet(ConsoleField.Out, null);
        ForceSet(ConsoleField.Error, null);
        /*
        Type type = typeof(Console);
        System.Reflection.FieldInfo _out = type.GetField("s_out", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        System.Reflection.FieldInfo _error = type.GetField("s_error", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        Debug.Assert(_out != null);
        Debug.Assert(_error != null);
        _out.SetValue(null, null);
        _error.SetValue(null, null);
        */
    }

    private static void SetOutAndErrorNull()
    {
        System.Console.SetOut(TextWriter.Null);
        System.Console.SetError(TextWriter.Null);
    }
}