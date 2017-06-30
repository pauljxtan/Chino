using System.Windows;
using Chino.ViewModel;
using System.Windows.Controls;

namespace Chino
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
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

            var viewModel = (MainViewModel)this.DataContext;
            viewModel.SelectedTagFilter = button.Content.ToString();
        }
    }
}