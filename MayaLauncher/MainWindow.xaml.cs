using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MayaLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Get access to the windows shield icon for showing admin rights requirements
        [DllImport("user32.dll")]
        static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        SelfLaunchable ThisLauncher;

        public MainWindow()
        {
            InitializeComponent();

            ThisLauncher = new SelfLaunchable();

            List<Launchable> launchables = new List<Launchable>();
            launchables.Add(ThisLauncher);
            launchables.AddRange(MayaLaunchable.FindAll());

            LaunchableList.ItemsSource = launchables;

            // Setup UAC shield on OkButton
            var image = LoadImage(IntPtr.Zero, "#106", 1, (int)SystemParameters.SmallIconWidth, (int)SystemParameters.SmallIconHeight, 0);
            var imageSource = Imaging.CreateBitmapSourceFromHIcon(image, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

            // Set button content from code
            var sp = new StackPanel
            {
                Orientation = Orientation.Horizontal,
            };
            sp.Children.Add(new Image { Source = imageSource, Stretch = Stretch.None });
            sp.Children.Add(new TextBlock { Text = "Set as Default", Margin = new Thickness(5, 0, 0, 0) });
            OkButton.Content = sp;

            OkButton.IsEnabled = false;

            UpdateSetupMessage();
        }

        private void LaunchableList_Loaded(object sender, RoutedEventArgs e)
        {
            LaunchableList.SelectDefaultLaunchable();
        }

        private void Button_Ok(object sender, RoutedEventArgs e)
        {
            string launcherExecutable = Process.GetCurrentProcess().MainModule.FileName;
            string launcherFolder = Path.GetDirectoryName(launcherExecutable);
            string extensionUtil = Path.Combine(launcherFolder, "MayaExtensionHandler.exe");

            string argument = "";
            if (LaunchableList.SelectedLaunchableIndex == 0)
            {
                argument = "launcher";
            }
            else
            {
                argument = LaunchableList.SelectedLaunchable.Version.ToString();
            }

            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = extensionUtil;
                process.StartInfo.Verb = "runas";
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.Arguments = argument;

                process.Start();

                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    string message = string.Format("Operation failed with error code: {0}", process.ExitCode);
                    MessageBox.Show(message, "Set file associations", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                ICollectionView view = CollectionViewSource.GetDefaultView(LaunchableList.ItemsSource);
                view.Refresh();
                UpdateSetupMessage();
            }
        }

        private void UpdateSetupMessage()
        {
            string message = "";

            if (ThisLauncher.IsDefaultLaunchable())
            {
                message += "Maya Launcher is the default handler for Maya files.";
            }
            else
            {
                message += "Maya Launcher is not set as the default handler for Maya files.";
            }

            if (!IsCurrentApplicationPinned())
            {
                message += "\nMaya Launcher works better when pinned to the TaskBar.";
            }

            FIleAssociationMessage.Content = message;
        }

        private void LaunchableList_SelectionChanged(object sender, System.EventArgs e)
        {
            Launchable SelectedLaunchable = LaunchableList.SelectedLaunchable;
            OkButton.IsEnabled = !SelectedLaunchable.IsDefaultLaunchable();
            Debug.WriteLine("Selected Launchable for association: " + SelectedLaunchable.DisplayName);
        }

        private bool IsCurrentApplicationPinned()
        {
            var currentPath = ThisLauncher.ExecutablePath;

            // folder with shortcuts
            string location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar");
            if (!Directory.Exists(location))
                return false;

            foreach (var file in Directory.GetFiles(location, "*.lnk"))
            {
                IWshShell shell = new WshShell();
                var lnk = shell.CreateShortcut(file) as IWshShortcut;
                if (lnk != null)
                {
                    // if there is shortcut pointing to current executable - it's pinned                                    
                    if (String.Equals(lnk.TargetPath, currentPath, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
