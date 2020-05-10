using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MayaLauncher
{
    class MayaLaunchable : Launchable
    {
        public override string Name => "Maya";

        public override int Version => _version;

        public override int Update => _update;

        public override string DisplayName
        {
            get
            {
                if (_update > 0)
                {
                    return string.Format("Maya {0} Update {1}{2}", _version, _update, IsDefaultLaunchable() ? " *" : "");
                }
                else
                {
                    return string.Format("Maya {0}{1}", _version, IsDefaultLaunchable() ? " *" : "");
                }
            }
        }

        public override string ExecutablePath => _executable;

        public override string[] Extensions => _extensions;

        public override BitmapSource ApplicationIcon => _applicationIcon;

        static readonly string[] _extensions = { "ma", "mb" };

        private static readonly string mayaOpenKey = @"\{0}\shell\open\command";
        private static readonly Regex mayaOpenRegex = new Regex("\".*?\"+|-?\\w+", RegexOptions.Compiled);

        private int _version;
        private int _update = 0;
        private string _executable;
        private BitmapSource _applicationIcon;

        private MayaLaunchable(int version, int update, string executable) : base()
        {
            _version = version;
            _update = update;
            _executable = executable;

            using (Icon ico = Icon.ExtractAssociatedIcon(executable))
            {
                _applicationIcon = Imaging.CreateBitmapSourceFromHIcon(ico.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
        }

        public override BitmapSource GetExtensionIcon(string extension)
        {
            throw new NotImplementedException();
        }

        public override bool IsDefaultLaunchable()
        {
            string key = string.Format(mayaOpenKey, "MayaAsciiFile");

            string result;
            if (GetValueForRootKey(key, null, out result))
            {
                var match = mayaOpenRegex.Match(result);
                if (match != null)
                {
                    string path = match.Value.Trim('"');
                    if (path.ToLower() == _executable.ToLower())
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static List<Launchable> FindAll()
        {
            List<Launchable> launchables = new List<Launchable>();
            for (int version = 2010; version < 2030; version++)
            {
                string value;
                if (GetMayaFolderForVersion(version, out value))
                {
                    string executable = Path.Combine(value, "bin", "maya.exe");
                    if (File.Exists(executable))
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(executable);
                        string[] components = info.FileVersion.Split(".");
                        if (components.Length > 1)
                        {
                            if (int.TryParse(components[1], out int update))
                            {
                                MayaLaunchable launchable = new MayaLaunchable(version, update, executable);
                                launchables.Add(launchable);
                            }
                        }
                    }
                }
            }

            if (launchables.Count > 0)
            {
                return launchables;
            }

            return null;
        }

        private static bool GetMayaFolderForVersion(int version, out string value)
        {
            value = null;
            try
            {
                string key = string.Format(@"SOFTWARE\Autodesk\Maya\{0}\Setup\InstallPath", version);
                GetValueForLocalMachineKey(key, "MAYA_INSTALL_LOCATION", out value);
            }
            catch { }
            return value != null;
        }

        private static bool GetValueForRootKey(string key, string value, out string result)
        {
            result = null;
            try
            {
                RegistryKey rk = Registry.ClassesRoot.OpenSubKey(key);
                if (rk != null)
                {
                    result = rk.GetValue(value) as string;
                }
                return result != null;
            }
            catch
            {
                return false;
            }
        }

        private static bool GetValueForLocalMachineKey(string key, string value, out string result)
        {
            result = null;
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(key);
                if (rk != null)
                {
                    result = rk.GetValue(value) as string;
                }
                return result != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
