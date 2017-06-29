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
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private SQLiteConnection _dbConnection;

        private string _currentPath = "";
        private DirectoryInfo _selectedDirectory = new DirectoryInfo();
        private FileInfo _selectedFile = new FileInfo();
        private List<string> _selectedFileTags = new List<string>();
        private Uri _selectedFileUri;
        private ObservableCollection<DirectoryInfo> _currentPathDirectories = new ObservableCollection<DirectoryInfo>();
        private ObservableCollection<FileInfo> _currentPathFiles = new ObservableCollection<FileInfo>();

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
            set { Set(ref _selectedDirectory, value); }
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
                    SelectedFileTags = Image.GetTags(SelectedFile.FileName);
                }
                catch (NullReferenceException)
                {
                    // TODO: handle
                }
            }
        }

        public List<string> SelectedFileTags
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

        public MainViewModel(IDataService dataService)
        {
            if (!File.Exists(Config.DatabasePath))
            {
                SQLiteConnection.CreateFile(Config.DatabasePath);
            }
            _dbConnection = new SQLiteConnection($"Data Source={Config.DatabasePath}");

            ShowOpenFolderDialogCommand = new RelayCommand(ShowOpenFolderDialog);
            GetPathContentsCommand = new RelayCommand(GetPathContents);
            GoToSelectedDirectoryCommand = new RelayCommand(GoToSelectedDirectory);
            GoToParentDirectoryCommand = new RelayCommand(GoToParentDirectory);

            // Default folder, for testing only
            CurrentPath = @"C:\Users\pjxta\Pictures\_pics";
        }

        public RelayCommand ShowOpenFolderDialogCommand { get; }
        public RelayCommand GetPathContentsCommand { get; }
        public RelayCommand GoToSelectedDirectoryCommand { get; }
        public RelayCommand GoToParentDirectoryCommand { get; }

        public void ShowOpenFolderDialog()
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
                //var fileNames = from file in Directory.EnumerateFiles(CurrentPath) where regex.Match(file).Success select Path.GetFileName(file);
                //var directoryNames = from dir in Directory.EnumerateDirectories(CurrentPath) select dir.Substring(dir.LastIndexOf("\\") + 1);

                var files = new List<FileInfo>();
                foreach (var file in Directory.EnumerateFiles(CurrentPath))
                {
                    if (regex.Match(file).Success)
                    {
                        var fileName = Path.GetFileName(file);
                        files.Add(new FileInfo(fileName));
                    }
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

        public void Dispose()
        {
            _dbConnection.Close();
        }

        ////public override void Cleanup()
        ////{
        ////    // Clean up if needed

        ////    base.Cleanup();
        ////}

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
    }
}