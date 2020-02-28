using Microsoft.Win32;
using System.Collections.Generic;

// Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Autodesk\Maya\2020\Setup\InstallPath
// MAYA_INSTALL_LOCATION

namespace MayaLauncher
{
    public struct MayaVersion
    {
        public string Name;
        public string Path;

        public static MayaVersion[] FindAll()
        {
            var result = new List<MayaVersion>();
            for (int i=2010; i<2030; i++)
            {
                dynamic value;
                if (TryGetMayaLocation(i, out value))
                {
                    var maya_version = new MayaVersion { Name = i.ToString(), Path = value };
                    System.Diagnostics.Debug.WriteLine((value as object).ToString());
                    result.Add(maya_version);
                }
            }

            return result.ToArray();
        }

        private static bool TryGetMayaLocation(int version, out dynamic value)
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
