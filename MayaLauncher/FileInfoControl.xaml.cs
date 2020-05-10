using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace MayaLauncher
{
    /// <summary>
    /// Interaction logic for FileInfoControl.xaml
    /// </summary>
    public partial class FileInfoControl : UserControl
    {
        public MayaFileParser.FileSummary Summary
        {
            get { return (MayaFileParser.FileSummary)GetValue(SummaryProperty); }
            set { SetValue(SummaryProperty, value); }
        }

        public static DependencyProperty SummaryProperty = DependencyProperty.Register("Summary", typeof(MayaFileParser.FileSummary), typeof(FileInfoControl));

        public FileInfoControl()
        {
            InitializeComponent();
            
        }
    }

    public class ListBoxHelper : DependencyObject
    {
        public static int GetAutoSizeItemCount(DependencyObject obj)
        {
            return (int)obj.GetValue(AutoSizeItemCountProperty);
        }

        public static void SetAutoSizeItemCount(DependencyObject obj, int value)
        {
            obj.SetValue(AutoSizeItemCountProperty, value);
        }

        public static readonly DependencyProperty AutoSizeItemCountProperty =
            DependencyProperty.RegisterAttached("AutoSizeItemCount", typeof(int), typeof(ListBoxHelper), new PropertyMetadata(0, OnAutoSizeItemCountChanged));

        static void OnAutoSizeItemCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var listBox = d as ListBox;

            // we set this to 0.0 so that we ddon't create any elements
            // before we have had a chance to modify the scrollviewer
            listBox.MaxHeight = 0.0;

            listBox.Loaded += OnListBoxLoaded;
        }

        static void OnListBoxLoaded(object sender, RoutedEventArgs e)
        {
            var listBox = sender as ListBox;

            var sv = Helper.GetChildOfType<ScrollViewer>(listBox);
            if (sv != null)
            {
                // limit the scrollviewer height so that the bare minimum elements are generated
                sv.MaxHeight = 1.0;

                var vsp = Helper.GetChildOfType<VirtualizingStackPanel>(listBox);
                if (vsp != null)
                {
                    vsp.SizeChanged += OnVirtualizingStackPanelSizeChanged;
                }
            }

            listBox.MaxHeight = double.PositiveInfinity;
        }

        static void OnVirtualizingStackPanelSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var vsp = sender as VirtualizingStackPanel;
            var lb = (ListBox)ItemsControl.GetItemsOwner(vsp);
            int maxCount = GetAutoSizeItemCount(lb);
            vsp.ScrollOwner.MaxHeight = vsp.Children.Count == 0 ? 1 : (((FrameworkElement)vsp.Children[0]).ActualHeight+1) * maxCount;
        }
    }

    public static class Helper
    {
        public static T GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }
}
