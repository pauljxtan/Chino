using GalaSoft.MvvmLight;
using Chino.Model;
using GalaSoft.MvvmLight.CommandWpf;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Chino.Model.Util;

namespace Chino.ViewModel
{
    public class TaggingViewModel : ViewModelBase
    {
        private static TaggingViewModel _instance = new TaggingViewModel();
        public static TaggingViewModel Instance { get { return _instance; } }

        private string _currentPath = null;
        private DirectoryInfo1 _selectedDirectory = null;
        private FileInfo1 _selectedFile = null;
        private ObservableCollection<TagInfo> _selectedFileTags = new ObservableCollection<TagInfo>();
        private Uri _selectedFileUri;
        private ObservableCollection<DirectoryInfo1> _currentPathDirectories = new ObservableCollection<DirectoryInfo1>();
        private ObservableCollection<FileInfo1> _currentPathFiles = new ObservableCollection<FileInfo1>();

        public string CurrentPath
        {
            get { return _currentPath; }
            set
            {
                Set(ref _currentPath, value);
                GetPathContents();
            }
        }

        public DirectoryInfo1 SelectedDirectory
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

        public FileInfo1 SelectedFile
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

        public ObservableCollection<DirectoryInfo1> CurrentPathDirectories
        {
            get { return _currentPathDirectories; }
            set { Set(ref _currentPathDirectories, value); }
        }

        public ObservableCollection<FileInfo1> CurrentPathFiles
        {
            get { return _currentPathFiles; }
            set { Set(ref _currentPathFiles, value); }
        }

        public TaggingViewModel()
        {
            ShowTaggingOpenFolderDialogCommand = new RelayCommand(ShowTaggingOpenFolderDialog);
            GetPathContentsCommand = new RelayCommand(GetPathContents);
            GoToSelectedDirectoryCommand = new RelayCommand(GoToSelectedDirectory);
            GoToParentDirectoryCommand = new RelayCommand(GoToParentDirectory);

            // Try to default to user's desktop, otherwise use the current directory
            try
            {
                CurrentPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            catch (Exception e)
            {
                CurrentPath = Directory.GetCurrentDirectory();
            }
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

        private void GetPathContents()
        {
            if (Directory.Exists(CurrentPath))
            {
                var regex = new Regex(Config.ImageFileRegex);

                var files = new List<FileInfo1>();
                foreach (var file in Directory.EnumerateFiles(CurrentPath).Where(f => regex.IsMatch(f)))
                {
                    var fileName = Path.GetFileName(file);
                    files.Add(new FileInfo1(fileName));
                }
                CurrentPathFiles = new ObservableCollection<FileInfo1>(files);

                var directories = new List<DirectoryInfo1>();
                // First item in list is parent directory                   
                var directoryName = "..";
                var numberOfSubdirs = Directory.EnumerateDirectories(CurrentPath).Count();
                var numberOfImages = Directory.EnumerateFiles(CurrentPath).Where(f => regex.Match(f).Success).Count();
                directories.Add(new DirectoryInfo1(directoryName, numberOfSubdirs, numberOfImages));
                foreach (var dir in Directory.EnumerateDirectories(CurrentPath))
                {
                    directoryName = dir.Substring(dir.LastIndexOf("\\") + 1);
                    try
                    {
                        numberOfSubdirs = Directory.EnumerateDirectories(dir).Count();
                        numberOfImages = Directory.EnumerateFiles(dir).Where(f => regex.Match(f).Success).Count();
                        directories.Add(new DirectoryInfo1(directoryName, numberOfSubdirs, numberOfImages));
                    }
                    catch
                    {
                        // TODO: handle not authorized exception here
                    }
                }
                CurrentPathDirectories = new ObservableCollection<DirectoryInfo1>(directories);
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
    }
}