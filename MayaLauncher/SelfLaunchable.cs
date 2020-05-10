using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace MayaLauncher
{
    class SelfLaunchable : Launchable
    {
        public override string Name => "MayaLauncher";

        public override int Version => _version;

        public override int Update => _update;

        public override string DisplayName
        {
            get
            {
                if (_update > 0)
                {
                    return string.Format("Launcher {0} Update {1}{2}", _version, _update, IsDefaultLaunchable() ? " *" : "");
                }
                else
                {
                    return string.Format("Launcher {0}{1}", _version, IsDefaultLaunchable() ? " *" : "");
                }
            }
        }

        public override string ExecutablePath => _executable;

        public override string[] Extensions => _extensions;

        public override BitmapSource ApplicationIcon => _applicationIcon;

        static readonly string[] _extensions = { "ma", "mb" };

        private static readonly string mayaOpenKey = @"\{0}\shell\open\command";
        private static readonly Regex mayaOpenRegex = new Regex("\".*?\"+|-?\\w+", RegexOptions.Compiled);

        private int _version = 2020;
        private int _update = 4;
        private string _executable;
        private BitmapSource _applicationIcon;

        public SelfLaunchable() : base()
        {
            _executable = Process.GetCurrentProcess().MainModule.FileName;

            using (Icon ico = Icon.ExtractAssociatedIcon(_executable))
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
    }
}
