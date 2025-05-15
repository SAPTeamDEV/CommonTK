// ----------------------------------------------------------------------------
//  <copyright file="Context.Application.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

using SAPTeam.CommonTK.ExecutionPolicy;

namespace SAPTeam.CommonTK;

public abstract partial class Context
{
    /// <summary>
    /// Provides the context of the application.
    /// </summary>
    public static class Application
    {
        private static InteractInterface interactinterface = DetectInteractionInterface();

        /// <summary>
        /// Gets or Sets the preferred interaction interface.
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
        /// Gets the full path of the application's executable file.
        /// </summary>
        public static string? FullPath
        {
            get
            {
                try
                {
                    return DetectExecutable();
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        public static string? Name
        {
            get
            {
                var path = FullPath;

                return string.IsNullOrEmpty(path) ? null : Path.GetFileNameWithoutExtension(path);
            }
        }

        /// <summary>
        /// Gets the application base directory.
        /// </summary>
        public static string? BaseDirectory
        {
            get
            {
                try
                {
                    var path = Path.GetDirectoryName(FullPath);

                    if (string.IsNullOrEmpty(path))
                    {
                        path = AppContext.BaseDirectory;
                    }

                    return path;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Detects the interaction interface of the current process.
        /// </summary>
        /// <returns></returns>
        public static InteractInterface DetectInteractionInterface()
        {
#if NET6_0_OR_GREATER
            // These OSs does not have a console support.
            var spcOS = OperatingSystem.IsAndroid()
                         || OperatingSystem.IsIOS();
#else
            var spcOS = false;
#endif

            if (spcOS)
            {
                return InteractInterface.UI;
            }

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
                var height = Console.WindowHeight;
                return height > 0 ? InteractInterface.Console : InteractInterface.UI;
            }
            catch (Exception)
            {
                return InteractInterface.UI;
            }
        }

        /// <summary>
        /// Gets the path to the application data directory based on the operating system.
        /// </summary>
        /// <param name="appName">
        /// The name of the application.
        /// </param>
        /// <returns>
        /// The path to the application data directory based on the operating system.
        /// </returns>
        public static string GetAppDataDirectory(string appName)
        {
            if (string.IsNullOrEmpty(appName))
            {
                var name = Name;

                if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentNullException(nameof(appName), "Application name cannot be null or empty.");
                }

                appName = name!;
            }

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                appName = appName.ToLowerInvariant();
            }

            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var unixHome = Environment.GetEnvironmentVariable("HOME") ?? Path.GetFullPath(".");
            var altAppData = Path.Combine(unixHome, ".config");

            var path = Path.Combine(string.IsNullOrEmpty(localAppData) ? altAppData : localAppData, appName);

            return path;
        }

        private static string? DetectExecutable()
        {
            string? path;

#if NET6_0_OR_GREATER
            // 1. .NET 6+: Environment.ProcessPath
            path = Environment.ProcessPath;
            if (IsValidExe(path))
            {
                return path;
            }
#endif

            // 2. Process.MainModule.FileName
            try
            {
                path = Process.GetCurrentProcess().MainModule?.FileName;
                if (IsValidExe(path))
                {
                    return path;
                }
            }
            catch
            {
                // ignore platform/permission issues
            }

            // 3. Entry assembly’s own Location
            Assembly? entryAsm = Assembly.GetEntryAssembly();
            if (entryAsm != null)
            {
#pragma warning disable IL3000
                path = entryAsm.Location;
#pragma warning disable IL3000
                if (IsValidExe(path))
                {
                    return path;
                }

                // 4. Combine BaseDirectory + assembly name + ".exe"
                var exeName = entryAsm.GetName().Name + ".exe";
                path = Path.Combine(AppContext.BaseDirectory, exeName);
                if (IsValidExe(path))
                {
                    return path;
                }
            }

            // 5. CommandLineArgs[0]
            var args = Environment.GetCommandLineArgs();
            if (args.Length > 0)
            {
                path = Path.GetFullPath(args[0]);
                if (IsValidExe(path))
                {
                    return path;
                }
            }

            return null;
        }

        private static bool IsValidExe(string? candidate)
            => !string.IsNullOrWhiteSpace(candidate)
               && File.Exists(candidate)
               && string.Equals(Path.GetExtension(candidate), ".exe", StringComparison.OrdinalIgnoreCase);

    }
}
