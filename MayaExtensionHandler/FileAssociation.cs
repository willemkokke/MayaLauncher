using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

namespace MayaExtensionHandler
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

        public static bool AssociateWithLauncher(string path)
        {
            try
            {
                string executablePath = Process.GetCurrentProcess().MainModule.FileName;

                foreach (var progId in progIds)
                {
                    SetDefaultForKey(string.Format("{0}\\DefaultIcon", progId), string.Format(defaultIconCommand, path));
                    SetDefaultForKey(string.Format("{0}\\shell\\open\\command", progId), string.Format(launcherOpenCommand, path));
                    DeleteKeyTree(string.Format("{0}\\shell\\Render", progId));
                }
                return true;
            }
            catch(Exception)
            {
                return false;
            }
        }

        public static bool AssociateWithMayaInstallation(string path)
        {
            try
            {
                string mayaPath = Path.Combine(path, "bin", "maya.exe");
                string renderPath = Path.Combine(path, "bin", "Render.exe");

                foreach (var progId in progIds)
                {
                    SetDefaultForKey(string.Format("{0}\\DefaultIcon", progId), string.Format(defaultIconCommand, mayaPath));
                    SetDefaultForKey(string.Format("{0}\\shell\\open\\command", progId), string.Format(mayaOpenCommand, mayaPath));
                    SetDefaultForKey(string.Format("{0}\\shell\\Render\\command", progId), string.Format(mayaRenderCommand, renderPath));
                }
                return true;
            }
            catch(Exception)
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
