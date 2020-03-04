using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows;
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
                //FileAssociation.AssociateWithMayaVersion(Maya.LatestVersion);
            }

            CreateJumpList();

            MainWindow = new MainWindow()
            {
                Width = 10,
                Height = 10,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = true,
                ShowActivated = true
            };

            MainWindow.Show();

            PipeManager = new NamedPipeManager("MayaLauncher");
            PipeManager.StartServer();
            PipeManager.ReceiveString += HandleNamedPipe_Message;

            ProcessCommandLineArgs(Environment.GetCommandLineArgs());
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

        private void ProcessCommandLineArgs(IList<string> args)
        {
            if (args == null || args.Count < 2)
                return;

            if ((args.Count > 2))
            {
                string command = args[1].ToLowerInvariant();

                //the first index always contains the location of the exe so we need to check the second index
                if (command == "/maya")
                {
                    string version = args[2];
                    Maya.Launch(version, AdjustEnvironment);
                }
                else if(command == "/open")
                {
                    string filename = args[2];
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
                            Maya.Launch(Maya.LatestVersion, AdjustEnvironment, filename);
                            break;
                    }
                }
            }
        }

        private void AdjustEnvironment(IDictionary<string, string> environment)
        {
            environment["MAYA_DISABLE_CIP"] = "1";
            environment["MAYA_DISABLE_CER"] = "1";

            PrependToEnvironment(environment, "MAYA_MODULE_PATH", @"E:\Projects\humain-mayatools");
        }

        private static void PrependToEnvironment(IDictionary<string, string> environment, string variable, string value)
        {
            if (environment.ContainsKey(variable))
            {
                value += (environment[variable] + ";");
            }
            environment[variable] = value;
        }
    }
}
