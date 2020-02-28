using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System.Windows.Shell;

namespace MayaLauncher
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Mutex Mutex { get; set; }

        public NamedPipeManager PipeManager { get; set; }

        public MayaVersion[] MayaVersions { get; set; }

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
            MayaVersions = MayaVersion.FindAll();

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

            foreach(var v in MayaVersions)
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
                //the first index always contains the location of the exe so we need to check the second index
                if ((args[1].ToLowerInvariant() == "/maya"))
                {
                    using (Process mayaProcess = new Process())
                    {
                        foreach (var v in MayaVersions)
                        {
                            if (v.Name == args[2])
                            {
                                var path = Path.Combine(v.Path, "bin", "maya.exe");
                                mayaProcess.StartInfo.UseShellExecute = false;
                                mayaProcess.StartInfo.FileName = path;
                                var env = mayaProcess.StartInfo.Environment;
                                PrependToEnvironment(env, "MAYA_MODULE_PATH", @"E:\Projects\humain-mayatools;");
                                PrependToEnvironment(env, "MAYA_DISABLE_CIP", "1");
                                PrependToEnvironment(env, "MAYA_DISABLE_CER", "1");
                            }
                        }
                        mayaProcess.Start();
                    }
                }
            }
        }

        private void PrependToEnvironment(IDictionary<string, string> env, string variable, string value)
        {
            if (env.ContainsKey(variable))
            {
                value += env[variable];
            }
            env[variable] = value;
        }
    }
}
