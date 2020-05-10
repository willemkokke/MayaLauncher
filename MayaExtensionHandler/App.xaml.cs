using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;
using Microsoft.Win32;

namespace MayaExtensionHandler
{
    /// <summary>
    /// The only thing this app does is adjust the registry settings for the maya file extensions.
    /// This requires admin priviledges, so for security, it does not take paths to executables, but finds the executables itself.
    /// This prevents adversaries from using this utility to associate maya files with their own executable, which poses a security risk
    /// </summary>
    public partial class App : Application
    {
        public const int SUCCESS = 0;
        public const int INVALID_ARGUMENTS = 1;
        public const int LAUNCHER_NOT_FOUND = 2;
        public const int MAYA_NOT_FOUND = 3;
        public const int REGISTRY_MODIFICATION_FAILED = 4;
        protected override void OnStartup(StartupEventArgs e)
        {
            int exitCode = ProcessCommandLineArgs(Environment.GetCommandLineArgs());
            Shutdown(exitCode);
        }

        private int ProcessCommandLineArgs(IList<string> args)
        {
            if (args == null || args.Count != 2)
                return INVALID_ARGUMENTS;

            // args[0] always is the location of this exe so we need to check args[1]
            string command = args[1].ToLowerInvariant();

            switch (command)
            {
                case "launcher":
                    Process[] launcher = Process.GetProcessesByName("MayaLauncher");
                    if (launcher.Length > 0)
                    {
                        string executable = launcher[0].MainModule.FileName;
                        if(!FileAssociation.AssociateWithLauncher(executable))
                        {
                            return REGISTRY_MODIFICATION_FAILED;
                        }
                        return SUCCESS;
                    }
                    return LAUNCHER_NOT_FOUND;
                default:
                    int version;
                    if (int.TryParse(command, out version))
                    {
                        dynamic value;
                        if (GetMayaLocation(version, out value))
                        {
                            string path = value;
                            if (!FileAssociation.AssociateWithMayaInstallation(path))
                            {
                                return REGISTRY_MODIFICATION_FAILED;
                            }
                            return SUCCESS;
                        }
                        return MAYA_NOT_FOUND;
                    }
                    return INVALID_ARGUMENTS;
            }
        }

        private static bool GetMayaLocation(int version, out dynamic value)
        {
            value = null;
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(string.Format(@"SOFTWARE\Autodesk\Maya\{0}\Setup\InstallPath", version));
                if (rk != null)
                    value = rk.GetValue("MAYA_INSTALL_LOCATION");
                return value != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
