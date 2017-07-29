using Chino.Model.Util;
using Chino.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chino.Controls
{
    public partial class Logs : UserControl
    {
        public Logs()
        {
            InitializeComponent();
        }

        // Enable DataGrid scrolling outside of scrollbar
        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
