using Chino.Model;
using Chino.Model.Util;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Linq;

namespace Chino.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private static MainViewModel _instance = new MainViewModel();
        public static MainViewModel Instance { get { return _instance; } }

        private ObservableCollection<TagInfo> _availableTags = new ObservableCollection<TagInfo>();
        private string _selectedTagFilter;
        private string _newTagName;

        public ObservableCollection<TagInfo> AvailableTags
        {
            get { return _availableTags; }
            set { Set(ref _availableTags, value); }
        }

        public string SelectedTagFilter
        {
            get { return _selectedTagFilter; }
            set
            {
                Set(ref _selectedTagFilter, value);
                ReloadAvailableTags();
            }
        }

        public string NewTagName
        {
            get { return _newTagName; }
            set { Set(ref _newTagName, value); }
        }

        public MainViewModel()
        {
            AddNewTagCommand = new RelayCommand(AddNewTag);
            SelectedTagFilter = "a";
        }

        public RelayCommand AddNewTagCommand { get; }

        private void AddNewTag()
        {
            ChinoRepository.AddTag(NewTagName);
            ReloadAvailableTags();
        }

        public void ReloadAvailableTags()
        {
            AvailableTags = new ObservableCollection<TagInfo>(ChinoRepository.GetAllTags()
                .Where(t => t.Name.StartsWith(SelectedTagFilter))
                .Select(t => new TagInfo(t.Name, ChinoRepository.GetImagesByTag(t.Name).Count)));
        }
    }
}
