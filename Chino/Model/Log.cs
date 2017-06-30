using Chino.Database;
using System;
using System.Collections.Generic;

namespace Chino.Model
{
    public enum LogLevel
    {
        Unknown,
        Trace,
        Debug,
        Info,
        Warning,
        Error
    }

    public enum LogEvent
    {
        // General
        AppStarted,
        AppTerminated,
        //TaggingTabSelected,
        //GalleryTabSelected,
        //LogsTabSelected,

        // Database
        ImageInserted,
        ImageInsertFailed,
        ImageDeleted,
        ImageDeleteFailed,
        TagInserted,
        TagInsertFailed,
        TagDeleted,
        TagDeleteFailed,
        ImageTagRelationInserted,
        ImageTagRelationInsertFailed,
        ImageTagRelationDeleted,
        ImageTagRelationDeleteFailed,
        LogInserted,
        LogInsertFailed,
        LogsDeleted,
        LogsDeleteFailed,

        // Errors
        Error = 200,

        // Generic
        Generic = 999
    }

    public class Log
    {
        public DateTime DateTime { get; set; }
        public LogLevel LogLevel { get; set; }
        public LogEvent LogEvent { get; set; }
        public string Message { get; set; }

        public Log(DateTime dateTime, LogLevel logLevel, LogEvent logEvent, string message = "")
        {
            DateTime = dateTime;
            LogLevel = logLevel;
            LogEvent = logEvent;
            Message = message;
        }

        public static void LogToDb(DateTime dateTime, LogLevel logLevel, LogEvent logEvent, string message = "")
        {
            ChinoDbHelper.InsertLog(dateTime, logLevel, logEvent, message);
        }

        public static List<Log> GetLogsForDate(DateTime dateTime)
        {
            return ChinoDbHelper.GetLogsForDate(dateTime);
        }
    }
}
