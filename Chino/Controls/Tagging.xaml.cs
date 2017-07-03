using Chino.Model.Util;
using Chino.ViewModel;
using MaterialDesignThemes.Wpf;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Chino.Controls
{
    public partial class Tagging : UserControl
    {
        private Point _mouseStartPoint;

        public Tagging()
        {
            InitializeComponent();
        }

        private void tagChip_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mouseStartPoint = e.GetPosition(null);
        }

        private void tagChip_MouseMove(object sender, MouseEventArgs e)
        {
            Point mousePoint = e.GetPosition(null);
            Vector dist = _mouseStartPoint - mousePoint;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(dist.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(dist.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                Chip tagChip = sender as Chip;
                var tag = tagChip.Content.ToString();

                var dragData = new DataObject(DataFormats.StringFormat, tag);
                DragDrop.DoDragDrop(tagChip, dragData, DragDropEffects.Copy);
            }
        }

        private void tagChip_DeleteClick(object sender, RoutedEventArgs e)
        {
            Chip tagChip = sender as Chip;
            var tagName = tagChip.Content.ToString();
            var viewmodel = DataContext as TaggingViewModel;
            viewmodel.RemoveTagFromSelectedImage(tagName);
        }

        private void fileTagDataGrid_DragEnter(object sender, DragEventArgs e)
        {
            var viewModel = DataContext as TaggingViewModel;
            if (!e.Data.GetDataPresent(typeof(TagInfo)) || sender == e.Source || viewModel.SelectedFile == null)
            {
                // TODO: this doesn't seem to be actually working...
                e.Effects = DragDropEffects.None;
            }
        }

        private void fileTagDataGrid_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TagInfo))) return;

            var tagInfo = e.Data.GetData(typeof(TagInfo)) as TagInfo;
            var viewModel = DataContext as TaggingViewModel;
            // Only add the tag if a file is actually selected
            if (viewModel.SelectedFile != null)
            {
                viewModel.AddTagToSelectedImage(tagInfo);
            }
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
