using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MayaLauncher
{
    /// <summary>
    /// Interaction logic for OpenWithLaunchableWindow.xaml
    /// </summary>
    public partial class OpenWithLaunchableWindow : Window
    {
        public Launchable SelectedLaunchable = null;
        public string[] Arguments;

        public OpenWithLaunchableWindow(MayaFileParser.FileSummary summary, string[] arguments)
        {
            Arguments = arguments;

            InitializeComponent();
    
            LaunchableList.ItemsSource = MayaLaunchable.FindAll();
            FileInfo.DataContext = summary;
            OkButton.IsEnabled = false;
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_Ok(object sender, RoutedEventArgs e)
        {
            if (LaunchableList.SelectedLaunchable != null)
            {
                if (SelectedLaunchable != null)
                {
                    SelectedLaunchable.Launch(Arguments);
                }
                Close();
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LaunchableList.SelectDefaultLaunchable();
        }

        private void LaunchableList_SelectionChanged(object sender, EventArgs e)
        {
            SelectedLaunchable = LaunchableList.SelectedLaunchable;
            OkButton.IsEnabled = true;
            Debug.WriteLine("Selected Launchable: " + SelectedLaunchable.DisplayName);
        }
    }
}
