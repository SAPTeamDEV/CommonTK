using System;
using System.Runtime.InteropServices;

namespace SAPTeam.CommonTK
{
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
        /// <param name="status">
        /// New text for current <see cref="IStatusProvider"/>.
        /// </param>
        /// <param name="progress">
        /// Type of progress bar.
        /// Note this feature must be supported by <see cref="StatusProvider.Current"/>.
        /// </param>
        public static void SetStatus(string status, ProgressBarType progress = ProgressBarType.None)
        {
            if (progress == ProgressBarType.None)
            {
                StatusProvider.Current.Write(status);
            }
            else if (StatusProvider.Current is IProgressStatusProvider ps)
            {
                ps.Write(status, progress);
            }
            else
            {
                throw new InvalidOperationException("Operation is not supported in current status provider.");
            }
        }

        /// <summary>
        /// Advances progress of status.
        /// </summary>
        /// <param name="value">
        /// New value of progress bar. if value is -1, uses default step value of progressbar.
        /// </param>
        public static void IncrementProgressStatus(int value = -1)
        {
            if (StatusProvider.Current is IProgressStatusProvider ps)
            {
                ps.Increment(value);
            }
            else
            {
                throw new InvalidOperationException("Operation is not supported in current status provider.");
            }
        }

        /// <summary>
        /// Clears text of current <see cref="IStatusProvider"/>.
        /// if class implements <see cref="IMultiStatusProvider"/> you can provide <paramref name="message"/>, otherwise it will ignored.
        /// </summary>
        /// <param name="message">
        /// A specific message that will be removed.
        /// </param>
        public static void ClearStatus(string message = null)
        {
            if (StatusProvider.Current is IMultiStatusProvider msp && message != null)
            {
                msp.Clear(message);
            }
            else
            {
                StatusProvider.Current.Clear();
            }
        }

        /// <summary>
        /// Sets new <see cref="IStatusProvider"/> class as global Status Provider.
        /// </summary>
        /// <param name="statusProvider">
        /// The new status provider object.
        /// </param>
        public static void AssignStatus(IStatusProvider statusProvider)
        {
            if (StatusProvider.Current != StatusProvider.Empty)
            {
                RemoveStatus();
            }

            if (StatusProvider.Current is not IMultiStatusProvider)
            {
                statusProvider.Clear();
            }
            StatusProvider.Current = statusProvider;
        }

        /// <summary>
        /// Resets and Removes current global <see cref="IStatusProvider"/>.
        /// </summary>
        public static void RemoveStatus()
        {
            ClearStatus();
            StatusProvider.Current = StatusProvider.Empty;
        }
    }
}