using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace MayaExtensionHandler
{
    // Computer\HKEY_CLASSES_ROOT\MayaAsciiFile
    // Computer\HKEY_CLASSES_ROOT\MayaBinaryFile

    // DefaultIcon .ma "C:\Program Files\Autodesk\Maya2018\bin\maya.exe",1
    // DefaultIcon .mb "C:\Program Files\Autodesk\Maya2018\bin\maya.exe",2
    // shell\open\command "C:\Program Files\Autodesk\Maya2018\bin\maya.exe" -file "%1"
    // shell\Render\command "C:\Program Files\Autodesk\Maya2018\bin\render.exe" "%1"

    public static class FileAssociation
    {
        private static string defaultIconCommand = "\"{0}\",{1}";
        private static string launcherOpenCommand = "\"{0}\" /open \"%1\"";
        private static string launcherInfoCommand = "\"{0}\" /info \"%1\"";
        private static string mayaOpenCommand = "\"{0}\" -file \"%1\"";
        private static string mayaRenderCommand = "\"{0}\" \"%1\"";

        private static string[] progIds = new string[] 
        {
            "MayaAsciiFile",
            "MayaBinaryFile",
        };

        private static int[] progIdsIconIds = new int[]
        {
            1,
            2,
        };

        private static string[] launcherIconIds = new string[]
        {
            "Resources\\ma.ico",
            "Resources\\mb.ico"
        };

        public static bool AssociateWithLauncher(string path)
        {
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    string progId = progIds[i];
                    string launcherIcon = launcherIconIds[i];
                    string folder = Path.GetDirectoryName(path);
                    string defaultIcon = $"\"{Path.Combine(folder, launcherIcon)}\"";
                    SetDefaultForKey(string.Format("{0}\\DefaultIcon", progId), defaultIcon);
                    SetDefaultForKey(string.Format("{0}\\shell\\open\\command", progId), string.Format(launcherOpenCommand, path));
                    SetDefaultForKey(string.Format("{0}\\shell\\Info\\command", progId), string.Format(launcherInfoCommand, path));
                    DeleteKeyTree(string.Format("{0}\\shell\\Render", progId));
                }
                UpdateShell();
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

                for (int i=0; i<2; i++)
                {
                    string progId = progIds[i];
                    SetDefaultForKey(string.Format("{0}\\DefaultIcon", progId), string.Format(defaultIconCommand, mayaPath, progIdsIconIds[i]));
                    SetDefaultForKey(string.Format("{0}\\shell\\open\\command", progId), string.Format(mayaOpenCommand, mayaPath));
                    SetDefaultForKey(string.Format("{0}\\shell\\Render\\command", progId), string.Format(mayaRenderCommand, renderPath));
                    DeleteKeyTree(string.Format("{0}\\shell\\Info", progId));
                }
                UpdateShell();
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

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern void SHChangeNotify(int wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);

        private static void UpdateShell()
        {
            int SHCNE_ASSOCCHANGED = 0x08000000;
            uint SHCNF_IDLIST = 0x0000;
            SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST, IntPtr.Zero, IntPtr.Zero);
        }
    }
}
