using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Interop;
using System.IO;
using System.ComponentModel;

namespace MayaLauncher
{
    /// <summary>
    /// Interaction logic for LauchableListControl.xaml
    /// </summary>
    public partial class LauchableListControl : UserControl
    {
        public event EventHandler SelectionChanged;
        public List<Launchable> ItemsSource
        {
            get { return (List<Launchable>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty =
          ListBox.ItemsSourceProperty.AddOwner(typeof(LauchableListControl));

        public Launchable SelectedLaunchable
        {
            get { return (Launchable)Container.SelectedItem; }
        }

        public int SelectedLaunchableIndex
        {
            get { return Container.SelectedIndex; }
        }

        public LauchableListControl()
        {
            InitializeComponent();
            Container.MaxHeight = 52 * 5 + 4;
        }

        public void SelectDefaultLaunchable()
        {
            if (ItemsSource != null)
            {
                for (int i = 0; i < ItemsSource.Count; i++)
                {
                    if (ItemsSource[i].IsDefaultLaunchable())
                    {
                        Container.SelectedIndex = i;
                    }
                }
            }
        }

        private void Container_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, EventArgs.Empty);
            }
        }
    }
}
