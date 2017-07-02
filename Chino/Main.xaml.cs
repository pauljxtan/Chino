using System.Windows;
using Chino.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;
using System;
using MaterialDesignThemes.Wpf;
using Chino.Model.Util;

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
                var tagInfo = new TagInfo(tagChip.Content.ToString(), (int)tagChip.Icon);
                var dragData = new DataObject(typeof(TagInfo), tagInfo);

                DragDrop.DoDragDrop(tagChip, dragData, DragDropEffects.Copy);
            }
        }
    }
}