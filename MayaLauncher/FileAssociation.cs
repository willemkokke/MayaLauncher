using Microsoft.Win32;
using System.Diagnostics;

namespace MayaLauncher
{
    // Computer\HKEY_CLASSES_ROOT\MayaAsciiFile
    // Computer\HKEY_CLASSES_ROOT\MayaBinaryFile

    // DefaultIcon "C:\Program Files\Autodesk\Maya2018\bin\maya.exe",2
    // shell\open\command "C:\Program Files\Autodesk\Maya2018\bin\maya.exe" -file "%1"
    // shell\Render\command "C:\Program Files\Autodesk\Maya2018\bin\render.exe" "%1"

    public static class FileAssociation
    {
        private static string defaultIconCommand = "\"{0},2";
        private static string launcherOpenCommand = "\"{0}\" /open \"%1\"";
        private static string mayaOpenCommand = "\"{0}\" -file \"%1\"";
        private static string mayaRenderCommand = "\"{0}\" \"%1\"";

        private static string[] progIds = new string[] 
        {
            "MayaAsciiFile",
            "MayaBinaryFile",
        };

        public static void AssociateWithLauncher()
        {
            string executablePath = Process.GetCurrentProcess().MainModule.FileName;

            foreach (var progId in progIds)
            {
                if (!string.IsNullOrEmpty(Maya.LatestVersion.InstallationPath))
                {
                    SetDefaultForKey(string.Format("{0}\\DefaultIcon", progId), string.Format(defaultIconCommand, Maya.LatestVersion.GetExectablePath()));
                }
                SetDefaultForKey(string.Format("{0}\\shell\\open\\command", progId), string.Format(launcherOpenCommand, executablePath));
                DeleteKeyTree(string.Format("{0}\\shell\\Render", progId));
            }
        }

        public static void AssociateWithMayaVersion(Maya.Version version)
        {
            foreach (var progId in progIds)
            {
                SetDefaultForKey(string.Format("{0}\\DefaultIcon", progId), string.Format(defaultIconCommand, version.GetExectablePath()));
                SetDefaultForKey(string.Format("{0}\\shell\\open\\command", progId), string.Format(mayaOpenCommand, version.GetExectablePath()));
                SetDefaultForKey(string.Format("{0}\\shell\\Render\\command", progId), string.Format(mayaRenderCommand, version.GetRenderPath()));
            }
        }

        private static bool GetDefaultForKey(string key, out string value)
        {
            value = null;
            try
            {
                RegistryKey rk = Registry.ClassesRoot.OpenSubKey(key);
                if (rk != null)
                {
                    value = rk.GetValue("MAYA_INSTALL_LOCATION") as string;
                }
                return value != null;
            }
            catch
            {
                return false;
            }
        }

        private static bool SetDefaultForKey(string key, string value)
        {
            try
            {
                RegistryKey rk = Registry.ClassesRoot.CreateSubKey(key, true);
                if (rk != null)
                {
                    rk.SetValue(null, value);
                    return true;
                }
            }
            catch (System.Exception e) { Debug.WriteLine(e.Message); }

            Debug.WriteLine(string.Format("Failed to set default value ({0}) for key ({1})", value, key));
            return false;
        }

        private static void DeleteKeyTree(string key)
        {
            Registry.ClassesRoot.DeleteSubKeyTree(key, false);
        }
    }
}
