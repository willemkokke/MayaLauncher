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
    public partial class InfoWindow : Window
    {
        public InfoWindow(MayaFileParser.FileSummary summary)
        {
            InitializeComponent();
            FileInfo.DataContext = summary;
            FileInfo.Expander.IsExpanded = true;
        }

        private void Button_Ok(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
