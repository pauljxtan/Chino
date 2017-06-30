using System.Windows;
using Chino.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using MaterialDesignThemes.Wpf;

namespace Chino
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Point _mouseStartPoint;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closing += (s, e) => ViewModelLocator.Cleanup();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void OnTagFilterClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button == null) return;

            var viewModel = DataContext as MainViewModel;
            viewModel.SelectedTagFilter = button.Content.ToString();
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

        private void FileTagList_DragEnter(object sender, DragEventArgs e)
        {
            var viewModel = DataContext as MainViewModel;
            if (!e.Data.GetDataPresent(DataFormats.StringFormat) || sender == e.Source || viewModel.SelectedFile == null)
            {
                // TODO: this doesn't seem to be actually working...
                e.Effects = DragDropEffects.None;
            }
        }

        private void FileTagList_Drop(object sender, DragEventArgs e)
        {
            var newTag = e.Data.GetData(DataFormats.StringFormat).ToString();
            var viewModel = DataContext as MainViewModel;
            // Only add the tag if a file is actually selected
            if (viewModel.SelectedFile != null)
            {
                viewModel.SelectedFileTags.Add(newTag);
                viewModel.UpdateFileTagsInDb();
            }
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}