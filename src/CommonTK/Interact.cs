using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK;

/// <summary>
/// Provides methods for Interact with User.
/// </summary>
public static class Interact
{
    [DllImport("user32.dll")] private static extern bool SetForegroundWindow(IntPtr hWnd);
    [DllImport("user32.dll")] private static extern bool SetFocus(IntPtr hWnd);

    /// <summary>
    /// Gets a string from user.
    /// </summary>
    /// <param name="request">Description of request</param>
    /// <returns>A string that contains user response.</returns>
    public static string Input(string request)
    {
        string result;

        Console.Write(request);
        result = Console.ReadLine();
        return result;
    }

    /// <summary>
    /// Bring the given window to top of other Windows.
    /// </summary>
    /// <param name="handle">handle of Window that will be Focused</param>
    public static void BringToFront(IntPtr handle)
    {
        SetForegroundWindow(handle);
        SetFocus(handle);
    }

    /// <summary>
    /// Set text of current <see cref="IStatusProvider"/> class.
    /// </summary>
    /// <param name="status">New text for current <see cref="IStatusProvider"/>.</param>
    public static void SetStatus(string status)
    {
        StatusProvider.Current.Write(status);
    }

    /// <summary>
    /// Clears text of current <see cref="IStatusProvider"/>.
    /// </summary>
    public static void ClearStatus()
    {
        StatusProvider.Current.Clear();
    }

    internal static void UnsetStatus()
    {
        ClearStatus();
        StatusProvider.Current = StatusProvider.Empty;
    }
}
