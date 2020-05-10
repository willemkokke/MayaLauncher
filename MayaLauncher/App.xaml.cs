using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shell;

namespace MayaLauncher
{
    public enum LaunchType
    {
        VersionFromFile,
        ChooseVersion,
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mutex Mutex { get; set; }

        public NamedPipeManager PipeManager { get; set; }

        private List<Launchable> MayaVersions = null;

        public App()
        {
            HandleRunningInstance();
        }

        private void HandleRunningInstance()
        {
            Mutex = new Mutex(true, "MayaLauncher", out bool isNewlyCreated);
            if (isNewlyCreated)
                return;

            var manager = new NamedPipeManager("MayaLauncher");
            var args = Environment.GetCommandLineArgs();
            _ = manager.Write(JsonSerializer.Serialize(args));

            Environment.Exit(0);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            MayaVersions = MayaLaunchable.FindAll();

           CreateJumpList();

            PipeManager = new NamedPipeManager("MayaLauncher");
            PipeManager.StartServer();
            PipeManager.ReceiveString += HandleNamedPipe_Message;

            ProcessCommandLineArgs(Environment.GetCommandLineArgs());
        }

        private Launchable FindLauchable(int version)
        {
            foreach(var v in MayaVersions)
            {
                if (v.Version == version)
                {
                    return v;
                }
            }
            return null;
        }

        private void CreateJumpList()
        {
            JumpList jumpList = JumpList.GetJumpList(Application.Current);
            if (jumpList == null)
            {
                jumpList = new JumpList();
                jumpList.ShowRecentCategory = true;
                JumpList.SetJumpList(Application.Current, jumpList);
            }
            foreach (var v in MayaVersions)
            {
                JumpTask mayaTask = new JumpTask();
                mayaTask.Title = v.DisplayName;
                mayaTask.Description = "Launch Maya " + v.DisplayName;
                mayaTask.ApplicationPath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");
                mayaTask.IconResourcePath = v.ExecutablePath;
                mayaTask.Arguments = "/maya " + v.Version.ToString();
                mayaTask.CustomCategory = "Launch Maya";
                jumpList.JumpItems.Add(mayaTask);
            }

            jumpList.Apply();
        }

        private void HandleNamedPipe_Message(string message)
        {
            var args = JsonSerializer.Deserialize<string[]>(message);
            Dispatcher.Invoke(() => ProcessCommandLineArgs(args));
        } 

        private void ProcessCommandLineArgs(IList<string> args)
        {
            if (args == null || args.Count < 2)
            {
                MainWindow = new MainWindow()
                {
                    ShowInTaskbar = true,
                    ShowActivated = true
                };

                MainWindow.Show();
                return;
            }

            //args[0] always is the location of this exe so we need to check args[1]
            string command = args[1].ToLowerInvariant();
            string param = args[2];

            if (command == "/maya")
            {
                int version;
                if (int.TryParse(param, out version))
                {
                    Launchable mayaVersion = FindLauchable(version);
                    if (mayaVersion != null)
                    {
                        mayaVersion.Launch();
                    }
                }
            }
            else if (command == "/open")
            {
                string filename = param;

                var summary = MayaFileParser.FileSummary.FromFile(filename);
                Debug.WriteLine(summary.ToString());

                if (summary == null)
                {
                    MessageBox.Show($"Could not parse {filename}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                LaunchType launchType = LaunchType.VersionFromFile;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    launchType = LaunchType.ChooseVersion;
                }

                Launchable fileMayaVersion = null;

                int version;
                if (int.TryParse(summary.InstallVersion, out version))
                {
                    fileMayaVersion = FindLauchable(version);
                    if (fileMayaVersion == null)
                    {
                        launchType = LaunchType.ChooseVersion;
                    }
                }
                else
                {
                    launchType = LaunchType.ChooseVersion;
                }

                string argument = $"-file \"{param}\"";

                switch (launchType)
                {
                    case LaunchType.VersionFromFile:
                        fileMayaVersion.Launch(new string[] { argument });
                        break;
                    case LaunchType.ChooseVersion:
                        string[] arguments = new string[] { argument };
                        OpenWithLaunchableWindow window = new OpenWithLaunchableWindow(summary, arguments);
                        window.Show();
                        break;
                }
            }
            else if (command == "/info")
            {
                string filename = param;

                var summary = MayaFileParser.FileSummary.FromFile(filename);
                if (summary != null)
                {
                    InfoWindow info = new InfoWindow(summary);
                    info.Show();
                }
            }
        }

        private void AdjustEnvironment(IDictionary<string, string> environment)
        {
            environment["MAYA_DISABLE_CIP"] = "1";
            environment["MAYA_DISABLE_CER"] = "1";

            PrependToEnvironment(environment, "MAYA_MODULE_PATH", @"C:\repositories\name-generator");
        }

        private static void PrependToEnvironment(IDictionary<string, string> environment, string variable, string value)
        {
            if (environment.ContainsKey(variable))
            {
                value += (environment[variable] + ";");
            }
            environment[variable] = value;
        }

        //[DllImport("user32")]
        //public static extern uint SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        //internal const int BCM_SETSHIELD = (0x1600 + 0x000C); //Elevated button

        //private static void AddShieldToButton(Button b)
        //{
        //    b.FlatStyle = FlatStyle.System;
        //    SendMessage(b.Handle, BCM_SETSHIELD, IntPtr.Zero, (IntPtr)1);
        //}
    }
}
