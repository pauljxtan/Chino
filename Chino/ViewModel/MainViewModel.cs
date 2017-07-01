using GalaSoft.MvvmLight;
using Chino.Model;
using GalaSoft.MvvmLight.CommandWpf;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Chino.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private string _currentPath = null;
        private DirectoryInfo _selectedDirectory = null;
        private FileInfo _selectedFile = null;
        private ObservableCollection<TagInfo> _selectedFileTags = new ObservableCollection<TagInfo>();
        private Uri _selectedFileUri;
        private ObservableCollection<DirectoryInfo> _currentPathDirectories = new ObservableCollection<DirectoryInfo>();
        private ObservableCollection<FileInfo> _currentPathFiles = new ObservableCollection<FileInfo>();
        private ObservableCollection<TagInfo> _availableTags = new ObservableCollection<TagInfo>();
        private string _selectedTagFilter;
        private TagInfo _selectedGalleryTag = null;
        private List<ImageInfo> _galleryImages = new List<ImageInfo>();
        private string _galleryRootDirectory = null;

        // Cache of all image URIs to avoid frequent traversals of the entire directory tree
        // It should remain up-to-date as long as the user does not add/delete/move files while the application is running
        // We should also have an option for the user to force a refresh
        private List<Uri> _allImageUris = null;

        public string CurrentPath
        {
            get { return _currentPath; }
            set
            {
                Set(ref _currentPath, value);
                GetPathContents();
            }
        }

        public DirectoryInfo SelectedDirectory
        {
            get { return _selectedDirectory; }
            set
            {
                Set(ref _selectedDirectory, value);
                if (_selectedDirectory != null)
                {
                    GoToSelectedDirectory();
                }
            }
        }

        public FileInfo SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                Set(ref _selectedFile, value);
                try
                {
                    SelectedFileUri = new Uri($"{CurrentPath}\\{SelectedFile.FileName}");
                    List<Tag> tags = ChinoRepository.GetTagsByImage(SelectedFile.FileName);
                    var tagInfos = new List<TagInfo>();
                    foreach (var tag in tags)
                    {
                        // Hardcoded number for now, calculate this later
                        tagInfos.Add(new TagInfo(tag.Name, 123));
                    }
                    SelectedFileTags = new ObservableCollection<TagInfo>(tagInfos);
                }
                catch (NullReferenceException)
                {
                    // TODO: handle
                }
            }
        }

        public ObservableCollection<TagInfo> SelectedFileTags
        {
            get { return _selectedFileTags; }
            set
            {
                Set(ref _selectedFileTags, value);
            }
        }

        public Uri SelectedFileUri
        {
            get { return _selectedFileUri; }
            set { Set(ref _selectedFileUri, value); }
        }

        public ObservableCollection<DirectoryInfo> CurrentPathDirectories
        {
            get { return _currentPathDirectories; }
            set { Set(ref _currentPathDirectories, value); }
        }

        public ObservableCollection<FileInfo> CurrentPathFiles
        {
            get { return _currentPathFiles; }
            set { Set(ref _currentPathFiles, value); }
        }

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

        public TagInfo SelectedGalleryTag
        {
            get { return _selectedGalleryTag; }
            set
            {
                Set(ref _selectedGalleryTag, value);
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

        public MainViewModel(IDataService dataService)
        {
            if (!File.Exists(Config.DatabasePath))
            {
                SQLiteConnection.CreateFile(Config.DatabasePath);
            }

            ShowTaggingOpenFolderDialogCommand = new RelayCommand(ShowTaggingOpenFolderDialog);
            ShowGalleriesOpenFolderDialogCommand = new RelayCommand(ShowGalleriesOpenFolderDialog);
            GetPathContentsCommand = new RelayCommand(GetPathContents);
            GoToSelectedDirectoryCommand = new RelayCommand(GoToSelectedDirectory);
            GoToParentDirectoryCommand = new RelayCommand(GoToParentDirectory);
            LoadGalleryCommand = new RelayCommand(LoadGallery);

            SelectedTagFilter = "a";

            // Try to default to user's desktop, otherwise use the current directory
            try
            {
                CurrentPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                GalleryRootDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            catch (Exception e)
            {
                CurrentPath = Directory.GetCurrentDirectory();
                GalleryRootDirectory = Directory.GetCurrentDirectory();
            }

            // TODO: Dynamically pick a default (probably just take the first alphabetically)
            SelectedGalleryTag = new TagInfo("abc", 123);

            // TESTING
        }

        public RelayCommand ShowTaggingOpenFolderDialogCommand { get; }
        public RelayCommand ShowGalleriesOpenFolderDialogCommand { get; }
        public RelayCommand GetPathContentsCommand { get; }
        public RelayCommand GoToSelectedDirectoryCommand { get; }
        public RelayCommand GoToParentDirectoryCommand { get; }
        public RelayCommand LoadGalleryCommand { get; }

        public void UpdateFileTagsInDb()
        {
            if (SelectedFile == null) return;
            var previousTags = ChinoRepository.GetTagsByImage(SelectedFile.FileName).Select(t => t.Name);
            var currentTags = SelectedFileTags.Select(ti => ti.TagName);

            // Any tags in previous but not current should be deleted from the database
            foreach (var tag in previousTags)
            {
                if (!currentTags.Contains(tag))
                {
                    ChinoRepository.RemoveTag(SelectedFile.FileName, tag);
                }
            }
            // Any tags in current but not previous should be added to the database
            foreach (var tag in currentTags)
            {
                if (!previousTags.Contains(tag))
                {
                    ChinoRepository.AddImageTagRelation(SelectedFile.FileName, tag);
                }
            }
        }

        private void ShowTaggingOpenFolderDialog()
        {
            var openFolderDialog = new CommonOpenFileDialog()
            {
                EnsureReadOnly = true,
                IsFolderPicker = true,
                AllowNonFileSystemItems = false,
                Multiselect = false,
                InitialDirectory = CurrentPath,
                Title = "Select folder"
            };
            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                CurrentPath = openFolderDialog.FileName;
            }
        }

        private void ShowGalleriesOpenFolderDialog()
        {
            var openFolderDialog = new CommonOpenFileDialog()
            {
                EnsureReadOnly = true,
                IsFolderPicker = true,
                AllowNonFileSystemItems = false,
                Multiselect = false,
                InitialDirectory = CurrentPath,
                Title = "Select folder"
            };
            if (openFolderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                GalleryRootDirectory = openFolderDialog.FileName;
            }
        }

        private void GetPathContents()
        {
            if (Directory.Exists(CurrentPath))
            {
                var regex = new Regex(Config.ImageFileRegex);

                var files = new List<FileInfo>();
                foreach (var file in Directory.EnumerateFiles(CurrentPath).Where(f => regex.IsMatch(f)))
                {
                    var fileName = Path.GetFileName(file);
                    files.Add(new FileInfo(fileName));
                }
                CurrentPathFiles = new ObservableCollection<FileInfo>(files);

                var directories = new List<DirectoryInfo>();
                // First item in list is parent directory                   
                var directoryName = "..";
                var numberOfSubdirs = Directory.EnumerateDirectories(CurrentPath).Count();
                var numberOfImages = Directory.EnumerateFiles(CurrentPath).Where(f => regex.Match(f).Success).Count();
                directories.Add(new DirectoryInfo(directoryName, numberOfSubdirs, numberOfImages));
                foreach (var dir in Directory.EnumerateDirectories(CurrentPath))
                {
                    directoryName = dir.Substring(dir.LastIndexOf("\\") + 1);
                    try
                    {
                        numberOfSubdirs = Directory.EnumerateDirectories(dir).Count();
                        numberOfImages = Directory.EnumerateFiles(dir).Where(f => regex.Match(f).Success).Count();
                        directories.Add(new DirectoryInfo(directoryName, numberOfSubdirs, numberOfImages));
                    }
                    catch
                    {
                        // TODO: handle not authorized exception here
                    }
                }
                CurrentPathDirectories = new ObservableCollection<DirectoryInfo>(directories);
            }
            else
            {
                MessageBox.Show($"The path {CurrentPath} does not exist");
            }
        }

        private void GoToSelectedDirectory()
        {
            CurrentPath = Path.GetFullPath($"{CurrentPath}\\{SelectedDirectory.DirectoryName}");
        }

        private void GoToParentDirectory()
        {
            CurrentPath = Path.GetFullPath(Directory.GetParent(CurrentPath).FullName);
        }

        private void ReloadAvailableTags()
        {
            AvailableTags = new ObservableCollection<TagInfo>(ChinoRepository.GetAllTags()
                .Where(t => t.Name.StartsWith(SelectedTagFilter))
                .Select(t => new TagInfo(t.Name, 123)));
        }

        private void LoadGallery()
        {
            // TODO: Support multiple tags!

            if (SelectedGalleryTag == null) return;

            if (_allImageUris == null) ReloadAllImageUris();

            var images = ChinoRepository.GetImagesByTag(SelectedGalleryTag.TagName);
            var galleryImages = new List<ImageInfo>();
            foreach (var uri in _allImageUris)
            {
                try
                {
                    if (ChinoRepository.GetTagNamesByImage(uri.Segments.Last()).Contains(SelectedGalleryTag.TagName))
                    {
                        galleryImages.Add(new ImageInfo(uri));
                    }
                }
                catch (Exception e)
                {
                    // TODO: Handle weird filenames here (e.g. with single quotes, etc.)
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

        public class DirectoryInfo
        {
            public string DirectoryName { get; set; }
            public int NumberOfSubdirs { get; set; }
            public int NumberOfImages { get; set; }

            public DirectoryInfo()
            {
            }

            public DirectoryInfo(string directoryName, int numberOfSubdirs, int numberOfImages)
            {
                DirectoryName = directoryName;
                NumberOfSubdirs = numberOfSubdirs;
                NumberOfImages = numberOfImages;
            }
        }

        public class FileInfo
        {
            public string FileName { get; set; }
            //public int FileSize { get; set; }

            public FileInfo()
            {
            }

            public FileInfo(string fileName)
            {
                FileName = fileName;
            }
        }

        public class ImageInfo
        {
            public Uri ImageUri { get; set; }

            public ImageInfo(Uri imageUri)
            {
                ImageUri = imageUri;
            }
        }

        public class TagInfo
        {
            public string TagName { get; set; }
            public int NumberOfImages { get; set; }

            public TagInfo()
            {
            }

            public TagInfo(string tagName, int numberOfImages)
            {
                TagName = tagName;
                NumberOfImages = numberOfImages;
            }
        }
    }
}