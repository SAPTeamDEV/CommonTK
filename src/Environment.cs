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

        private static InteractInterface interactinterface = CheckConsole();

        private static InteractInterface CheckConsole()
        {
            if (Environment.OSVersion.Platform == PlatformID.Unix)
            {
                // Unix console behavior
                if ((int)Console.BackgroundColor == -1)
                {
                    // There is no console
                    return InteractInterface.UI;
                }
                else
                {
                    return InteractInterface.Console;
                }
            }

            // Windows console behavior
            try
            {
                int height = Console.WindowHeight;
                return height > 0 ? InteractInterface.Console : InteractInterface.UI;
            }
            catch (Exception)
            {
                return InteractInterface.UI;
            }
        }

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
