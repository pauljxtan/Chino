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
    public class LogsViewModel : ViewModelBase
    {
        private static LogsViewModel _instance = new LogsViewModel();
        public static LogsViewModel Instance { get { return _instance; } }

        public List<Log> Logs { get; set; }

        public LogsViewModel()
        {
            ReloadLogsCommand = new RelayCommand(ReloadLogs);

            Logs = ChinoRepository.GetLogsForDate(DateTime.Today).OrderByDescending(log => log.DateTime).Take(100).ToList();
        }

        public RelayCommand ReloadLogsCommand { get; }

        private void ReloadLogs()
        {
        }


    }
}