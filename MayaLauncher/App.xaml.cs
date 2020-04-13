using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
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
        LatestVersion,
        ChooseVersion,
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mutex Mutex { get; set; }

        public NamedPipeManager PipeManager { get; set; }

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
            Maya.Initalise();

            if (Maya.Versions.Count > 0)
            {
                FileAssociation.AssociateWithLauncher();
            }

            CreateJumpList();

            PipeManager = new NamedPipeManager("MayaLauncher");
            PipeManager.StartServer();
            PipeManager.ReceiveString += HandleNamedPipe_Message;

            bool showWindow = ProcessCommandLineArgs(Environment.GetCommandLineArgs());
            if (showWindow)
            {
                MainWindow = new MainWindow()
                {
                    ShowInTaskbar = true,
                    ShowActivated = true
                };

                MainWindow.Show();
            }
            else
            {
                Shutdown();
            }

        }

        private void CreateJumpList()
        {
            JumpList jumpList = new JumpList();
            JumpList.SetJumpList(Application.Current, jumpList);

            foreach(var v in Maya.Versions)
            {
                JumpTask mayaTask = new JumpTask();
                mayaTask.Title = v.Name;
                mayaTask.Description = "Launch a new window";
                mayaTask.ApplicationPath = Assembly.GetEntryAssembly().Location.Replace(".dll", ".exe");
                mayaTask.Arguments = "/maya " + v.Name;
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

        private bool ProcessCommandLineArgs(IList<string> args)
        {
            if (args == null || args.Count <= 2)
                return true;

            //args[0] always is the location of this exe so we need to check args[1]
            string command = args[1].ToLowerInvariant();
            string param = args[2];

            if (command == "/maya")
            {
                string version = param;
                Maya.Launch(version, AdjustEnvironment);
                return false;
            }
            else if (command == "/open")
            {
                string filename = param;
                LaunchType launchType = LaunchType.VersionFromFile;

                if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
                {
                    launchType = LaunchType.LatestVersion;
                }
                else if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
                {
                    launchType = LaunchType.LatestVersion;
                }

                var fileVersion = MayaFileVersion.FromFile(filename);
                if (fileVersion != null)
                {
                    Debug.WriteLine(fileVersion.Requires);
                    Debug.WriteLine(fileVersion.Product);
                    Debug.WriteLine(fileVersion.Version);
                    Debug.WriteLine(fileVersion.Cut);
                }

                switch (launchType)
                {
                    case LaunchType.VersionFromFile:
                        break;
                    case LaunchType.ChooseVersion:
                        break;
                    case LaunchType.LatestVersion:
                        //Maya.Launch(Maya.LatestVersion, AdjustEnvironment, filename);
                        break;
                }
                return false;
            }
            else 
            {
                // unknown command
                return true;
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
