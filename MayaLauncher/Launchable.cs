using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Media.Imaging;

namespace MayaLauncher
{
    public abstract class Launchable
    {
        public abstract string Name
        {
            get;
        }

        public abstract int Version
        {
            get;
        }

        public abstract int Update
        {
            get;
        }

        public abstract string DisplayName
        {
            get;
        }

        public abstract string ExecutablePath
        {
            get;
        }

        public abstract string[] Extensions
        {
            get;
        }

        public abstract BitmapSource ApplicationIcon
        {
            get;
        }

        public virtual bool IsDefaultLaunchable()
        {
            return false;
        }

        public abstract BitmapSource GetExtensionIcon(string extension);

        public bool Launch(string[] arguments = null)
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = ExecutablePath;

                if (arguments != null && arguments.Length > 0)
                {
                    process.StartInfo.Arguments = string.Join(" ", arguments);
                }

                var environment = process.StartInfo.Environment;
                AdjustEnvironment(environment);

                return process.Start();
            }
        }

        protected virtual void AdjustEnvironment(IDictionary<string, string> environment)
        {

        }
    }
}
