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
        private string _selectedDirectory = "";
        private string _selectedFile = "";
        private List<string> _selectedFileTags = new List<string>();
        private Uri _selectedFileUri;
        private ObservableCollection<string> _currentPathDirectories = new ObservableCollection<string>();
        private ObservableCollection<string> _currentPathFiles = new ObservableCollection<string>();

        public string CurrentPath
        {
            get { return _currentPath; }
            set
            {
                Set(ref _currentPath, value);
                GetPathContents();
            }
        }

        public string SelectedDirectory
        {
            get { return _selectedDirectory; }
            set { Set(ref _selectedDirectory, value); }
        }

        public string SelectedFile
        {
            get { return _selectedFile; }
            set
            {
                Set(ref _selectedFile, value);
                SelectedFileUri = new Uri($"{CurrentPath}\\{SelectedFile}");
                SelectedFileTags = Image.GetTags(SelectedFile);
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

        public ObservableCollection<string> CurrentPathDirectories
        {
            get { return _currentPathDirectories; }
            set { Set(ref _currentPathDirectories, value); }
        }

        public ObservableCollection<string> CurrentPathFiles
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
                var fileNames = from file in Directory.EnumerateFiles(CurrentPath) where regex.Match(file).Success select Path.GetFileName(file);
                var directoryNames = from dir in Directory.EnumerateDirectories(CurrentPath) select dir.Substring(dir.LastIndexOf("\\") + 1);
                CurrentPathFiles = new ObservableCollection<string>(fileNames);
                CurrentPathDirectories = new ObservableCollection<string>(directoryNames);
            }
            else
            {
                MessageBox.Show($"The path {CurrentPath} does not exist");
            }
        }

        private void GoToSelectedDirectory()
        {
            CurrentPath = $"{CurrentPath}\\{SelectedDirectory}";
        }

        private void GoToParentDirectory()
        {
            CurrentPath = Directory.GetParent(CurrentPath).FullName;
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
    }
}