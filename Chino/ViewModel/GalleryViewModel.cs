using GalaSoft.MvvmLight;
using Chino.Model;
using GalaSoft.MvvmLight.CommandWpf;
using System.IO;
using System.Linq;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Chino.Model.Util;
using System.Collections.ObjectModel;

namespace Chino.ViewModel
{
    public class GalleryViewModel : ViewModelBase
    {
        private static GalleryViewModel _instance = new GalleryViewModel();
        public static GalleryViewModel Instance { get { return _instance; } }

        private ObservableCollection<TagInfo> _selectedGalleryTags = null;
        private List<ImageInfo> _galleryImages = new List<ImageInfo>();
        private string _galleryRootDirectory = null;

        // Cache of all image URIs to avoid frequent traversals of the entire directory tree
        // It should remain up-to-date as long as the user does not add/delete/move files while the application is running
        // We should also have an option for the user to force a refresh
        private List<Uri> _allImageUris = null;

        public ObservableCollection<TagInfo> SelectedGalleryTags
        {
            get { return _selectedGalleryTags; }
            set
            {
                Set(ref _selectedGalleryTags, value);
                LoadGallery();
            }
        }

        public List<ImageInfo> GalleryImages
        {
            get { return _galleryImages; }
            set
            {
                Set(ref _galleryImages, value);
            }
        }

        public string GalleryRootDirectory
        {
            get { return _galleryRootDirectory; }
            set
            {
                Set(ref _galleryRootDirectory, value);
                ReloadAllImageUris();
                LoadGallery();
            }
        }

        public GalleryViewModel()
        {
            ShowGalleriesOpenFolderDialogCommand = new RelayCommand(ShowGalleriesOpenFolderDialog);
            LoadGalleryCommand = new RelayCommand(LoadGallery);

            // Try to default to user's desktop, otherwise use the current directory
            try
            {
                GalleryRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            catch (Exception e)
            {
                GalleryRootDirectory = Directory.GetCurrentDirectory();
            }

            // Initialize to the alphabetically first tag
            SelectedGalleryTags = new ObservableCollection<TagInfo>() { MainViewModel.Instance.AvailableTags.First() };
        }

        public RelayCommand ShowGalleriesOpenFolderDialogCommand { get; }
        public RelayCommand LoadGalleryCommand { get; }

        private void ShowGalleriesOpenFolderDialog()
        {
            var openFolderDialog = new CommonOpenFileDialog()
            {
                EnsureReadOnly = true,
                IsFolderPicker = true,
                AllowNonFileSystemItems = false,
                Multiselect = false,
                InitialDirectory = GalleryRootDirectory,
                Title = "Select folder"
            };
            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GalleryRootDirectory = openFolderDialog.FileName;
            }
        }

        public void LoadGallery()
        {
            // TODO: Support multiple tags!

            if (SelectedGalleryTags == null || SelectedGalleryTags.Count == 0) return;

            if (_allImageUris == null) ReloadAllImageUris();

            var galleryImages = new List<ImageInfo>();
            foreach (var uri in _allImageUris)
            {
                try
                {
                    var imageTags = ChinoRepository.GetTagNamesByImage(uri.Segments.Last());
                    if (imageTags.Count == 0) continue;
                    var allTagsPresent = true;
                    foreach (var tag in SelectedGalleryTags)
                    {
                        if (!imageTags.Contains(tag.TagName))
                        {
                            allTagsPresent = false;
                        }
                    }
                    if (allTagsPresent)
                    {
                        galleryImages.Add(new ImageInfo(uri));
                    }
                }
                catch (Exception e)
                {
                    // TODO: Properly handle weird filenames here (e.g. with single quotes, etc.)
                    // For now just ignore it...
                }
            }
            GalleryImages = galleryImages;
        }

        // Traverses all files below the root gallery folder - use sparingly
        private void ReloadAllImageUris()
        {
            if (GalleryRootDirectory == null) return;

            var regex = new Regex(Config.ImageFileRegex);
            _allImageUris = Directory.GetFiles(GalleryRootDirectory, "*", SearchOption.AllDirectories)
                .Where(f => regex.IsMatch(f))
                .Select(f => new Uri(Path.GetFullPath(f.Replace(@"'", @"\\'"))))
                .ToList();
        }

    }
}