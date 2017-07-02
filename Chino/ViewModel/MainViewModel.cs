using Chino.Model;
using Chino.Model.Util;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chino.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private static MainViewModel _instance = new MainViewModel();
        public static MainViewModel Instance { get { return _instance; } }

        private ObservableCollection<TagInfo> _availableTags = new ObservableCollection<TagInfo>();
        private string _selectedTagFilter;

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

        public MainViewModel()
        {

            SelectedTagFilter = "a";

        }

        private void ReloadAvailableTags()
        {
            AvailableTags = new ObservableCollection<TagInfo>(ChinoRepository.GetAllTags()
                .Where(t => t.Name.StartsWith(SelectedTagFilter))
                .Select(t => new TagInfo(t.Name, 123)));
        }

    }
}
