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
    public partial class Gallery : UserControl
    {
        public Gallery()
        {
            InitializeComponent();
        }
        private void selectedTagsZone_DragEnter(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TagInfo)) || sender == e.Source)
            {
                // TODO: this doesn't seem to be actually working...
                e.Effects = DragDropEffects.None;
            }
        }

        private void selectedTagsZone_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(TagInfo))) return;

            var tagInfo = e.Data.GetData(typeof(TagInfo)) as TagInfo;
            var viewModel = DataContext as GalleryViewModel;
            // Add if tag is not already selected
            
            if (viewModel.SelectedGalleryTags.Count(t => t.TagName == tagInfo.TagName) == 0)
            {
                viewModel.SelectedGalleryTags.Add(tagInfo);
                viewModel.LoadGallery();
            }
        }
    }
}
