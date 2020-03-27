using Microsoft.Win32;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

// Computer\HKEY_LOCAL_MACHINE\SOFTWARE\Autodesk\Maya\2020\Setup\InstallPath
// MAYA_INSTALL_LOCATION

namespace MayaLauncher
{
    public static class Maya
    {
        public struct Version
        {
            public int Year { get; }
            public int ServicePack { get; }
            public string Name { get; }
            public string InstallationPath { get; }

            internal Version(int year, string installationPath)
            {
                Year = year;
                ServicePack = 0;
                InstallationPath = installationPath;
                Name = year.ToString();

                var info = FileVersionInfo.GetVersionInfo(GetExectablePath());
                var components = info.FileVersion.Split(".");
                if (components.Length > 1)
                {
                    if (int.TryParse(components[1], out int servicePack))
                    {
                        ServicePack = servicePack;
                    }
                }

                Name += "." + ServicePack.ToString();
            }

            public string GetExectablePath()
            {
                return Path.Combine(InstallationPath, "bin", "maya.exe");
            }

            public string GetRenderPath()
            {
                return Path.Combine(InstallationPath, "bin", "Render.exe");
            }
        }

        public static Version LatestVersion { get => latestVersion; }
        public static List<Version> Versions { get => versions; }

        public delegate void EnvironmentCallBack(IDictionary<string, string> environment);

        private static List<Version> versions;
        private static Version latestVersion;

        public static void Initalise()
        {
            versions = new List<Version>();
            for (int i = 2010; i < 2030; i++)
            {
                dynamic value;
                if (GetMayaLocation(i, out value))
                {
                    var version = new Version(i, value);
                    Versions.Add(version);
                }
            }

            if (Versions.Count > 0)
            {
                latestVersion = Versions[Versions.Count - 1];
            }
        }

        public static Version? Find(string version)
        {
            foreach (var v in Versions)
            {
                if (v.Name == version)
                {
                    return v;
                }
            }

            return null;
        }

        public static void Launch(string version, EnvironmentCallBack callback = null, string scene = "")
        {
            var v = Find(version);
            if (v != null)
            {
                Launch(v.Value, callback, scene);
            }
        }

        public static void Launch(Version version, EnvironmentCallBack callback = null, string scene = "")
        {
            using (Process mayaProcess = new Process())
            {
                mayaProcess.StartInfo.UseShellExecute = false;
                mayaProcess.StartInfo.FileName = version.GetExectablePath();

                if (!string.IsNullOrEmpty(scene))
                {
                    mayaProcess.StartInfo.Arguments = string.Format("-file \"{0}\"", scene);
                }

                if (callback != null)
                {
                    var env = mayaProcess.StartInfo.Environment;
                    callback(env);
                }

                mayaProcess.Start();
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
