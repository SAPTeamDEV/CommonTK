using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SAPTeam.CommonTK
{
    public abstract partial class Context
    {
        [DllImport("kernel32.dll")] private static extern IntPtr GetConsoleWindow();
        private static InteractInterface interactinterface = GetConsoleWindow() != IntPtr.Zero ? InteractInterface.Console : InteractInterface.UI;

        /// <summary>
        /// Gets or Sets the process interaction Interface.
        /// <para>
        /// Property setter Action Group: global.interface
        /// </para>
        /// </summary>
        public static InteractInterface Interface
        {
            get => interactinterface;
            set
            {
                QueryGroup(ActionGroup(ActionScope.Global, "interface"));
                interactinterface = value;
            }
        }

        /// <summary>
        /// Gets the full path of the executable application.
        /// </summary>
        public static string ExecutablePath
        {
            get
            {
                var file = AppDomain.CurrentDomain.FriendlyName;
                if (File.Exists(file))
                {
                    return file;
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        public static string ApplicationTitle => Path.GetFileName(AppDomain.CurrentDomain.FriendlyName);

        /// <summary>
        /// Gets the root application directory.
        /// </summary>
        public static string ApplicationDirectory => AppDomain.CurrentDomain.BaseDirectory;
    }
}
