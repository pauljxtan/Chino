using Chino.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Chino.Database
{
    public static class ChinoDbHelper
    {
        //private static SqliteHelper _sqliteHelper = new SqliteHelper(Config.DefaultDatabasePath);
        //private static SqliteHelper _sqliteHelper = new SqliteHelper($"{Directory.GetCurrentDirectory()}\\chino.sqlite");
        private static SqliteHelper _sqliteHelper = new SqliteHelper($"{AppDomain.CurrentDomain.BaseDirectory}\\chino.sqlite");

        #region Gets

        public static List<Image> GetAllImages()
        {
            var images = new List<Image>();
            var sql = "SELECT image_name FROM images;";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                while (reader.Read())
                {
                    var imageName = reader["image_name"].ToString();
                    var image = new Image(imageName, GetTagsByImage(imageName));
                    images.Add(image);
                }
            }
            return images;
        }

        public static List<Tag> GetAllTags()
        {
            var tags = new List<Tag>();
            var sql = @"SELECT tag_name FROM tags;";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                while (reader.Read())
                {
                    var tag = new Tag(reader["tag_name"].ToString());
                    tags.Add(tag);
                }
            }
            return tags;
        }

        public static List<Tuple<Image, Tag>> GetAllImageTagRelations()
        {
            var relations = new List<Tuple<Image, Tag>>();
            var sql = "SELECT image_name, tag_name FROM imagetags;";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                while (reader.Read())
                {
                    var image = new Image(reader["image_name"].ToString());
                    var tag = new Tag(reader["tag_name"].ToString());
                    relations.Add(new Tuple<Image, Tag>(image, tag));
                }
            }
            return relations;
        }

        public static List<Log> GetLogsForDate(DateTime dateTime)
        {
            var logs = new List<Log>();
            var sql = $"SELECT datetime, log_level, log_event, message FROM logs WHERE date(datetime) = {dateTime.ToString("yyyy-MM-dd")};";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                while (reader.Read())
                {
                    var logLevel = LogLevel.Unknown;
                    var logEvent = LogEvent.Generic;
                    Enum.TryParse(reader["log_level"].ToString(), out logLevel);
                    Enum.TryParse(reader["log_event"].ToString(), out logEvent);
                    var log = new Log(DateTime.Parse(reader["datetime"].ToString()), logLevel, logEvent, reader["message"].ToString());
                    logs.Add(log);
                }
            }
            return logs;
        }

        public static List<Tag> GetTagsByImage(string imageName)
        {
            return GetTagNamesByImage(imageName).Select(tn => new Tag(tn)).ToList();
        }

        public static List<string> GetTagNamesByImage(string imageName)
        {
            var tags = new List<string>();

            var sql = new StringBuilder();
            sql.Append(@"SELECT t.tag_name ");
            sql.Append(@"FROM tags AS t ");
            sql.Append(@"INNER JOIN imagetags AS it ");
            sql.Append(@"ON t.tag_name = it.tag_name ");
            sql.Append(@"INNER JOIN images AS i ");
            sql.Append(@"ON it.image_name = i.image_name ");
            sql.Append($"WHERE i.image_name = '{imageName}';");

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql.ToString()))
            {
                while (reader.Read())
                {
                    tags.Add(reader["tag_name"].ToString());
                }
            }
            return tags;
        }

        public static List<Image> GetImagesByTag(string tagName)
        {
            return GetImageNamesByTag(tagName).Select(i => new Image(i, GetTagsByImage(i))).ToList();
        }

        public static List<string> GetImageNamesByTag(string tagName)
        {
            var images = new List<string>();

            var sql = new StringBuilder();
            sql.Append(@"SELECT i.image_name ");
            sql.Append(@"FROM images AS i ");
            sql.Append(@"INNER JOIN imagetags AS it ");
            sql.Append(@"ON i.image_name = it.image_name ");
            sql.Append(@"INNER JOIN tags AS t ");
            sql.Append(@"ON it.tag_name = t.tag_name ");
            sql.Append($"WHERE t.tag_name = '{tagName}';");

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql.ToString()))
            {
                while (reader.Read())
                {
                    images.Add(reader["image_name"].ToString());
                }
            }
            return images;
        }

        #endregion

        #region Checks

        public static bool ImageExists(string imageName)
        {
            var sql = $"SELECT image_name FROM images WHERE image_name = '{imageName}';";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                return reader.HasRows;
            }
        }

        public static bool TagExists(string tagName)
        {
            var sql = $"SELECT tag_name FROM tags WHERE tag_name = '{tagName}';";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                return reader.HasRows;
            }
        }

        public static bool ImageTagRelationExists(string imageName, string tagName)
        {
            var sql = $"SELECT image_name, tag_name FROM imagetags WHERE image_name = '{imageName}' AND tag_name = '{tagName}';";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                return reader.HasRows;
            }
        }

        #endregion

        #region Inserts

        // Inserts should all be logged (except log inserts)

        public static bool InsertImage(string imageName)
        {
            if (ImageExists(imageName)) return true;
            var sql = $"INSERT INTO images (image_name) VALUES ('{imageName}');";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.ImageInserted, $"Inserted image: {imageName}");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.ImageInsertFailed, $"Failed to insert image: {imageName}");
            }
            return success;
        }

        public static bool InsertTag(string tagName)
        {
            if (TagExists(tagName)) return true;

            var sql = $"INSERT INTO tags (tag_name) VALUES ('{tagName}');";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.TagInserted, $"Inserted tag: {tagName}");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.TagInsertFailed, $"Failed to insert tag: {tagName}");
            }
            return success;
        }

        public static bool InsertImageTagRelation(string imageName, string tagName)
        {
            var sql = $"INSERT INTO imagetags (image_name, tag_name) VALUES ('{imageName}', '{tagName}');";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.ImageTagRelationInserted, $"Inserted image-tag relation: {imageName}, {tagName}");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.ImageTagRelationInsertFailed, $"Failed to insert image-tag relation: {imageName}, {tagName}");
            }
            return success;
        }

        public static bool InsertLog(DateTime dateTime, LogLevel logLevel, LogEvent logEvent, string message)
        {
            var dateTimeStr = dateTime.ToString("yyyy-MM-dd HH:mm:ss");

            var sql = $"INSERT INTO logs (datetime, log_level, log_event, message) VALUES ('{dateTimeStr}', '{logLevel.ToString()}', '{logEvent.ToString()}', '{message}');";

            // Don't log this - it's recursive! :)

            return _sqliteHelper.ExecuteNonQuery(sql);
        }

        #endregion

        #region Deletes

        // Deletes should all be logged

        public static bool DeleteImage(string imageName)
        {
            if (!ImageExists(imageName)) return true;
            var sql = $"DELETE FROM images WHERE image_name = '{imageName}';";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.ImageDeleted, $"Deleted image: {imageName}");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.ImageDeleteFailed, $"Failed to delete image: {imageName}");
            }
            return success;
        }

        public static bool DeleteTag(string tagName)
        {
            if (!TagExists(tagName)) return true;

            var sql = $"DELETE FROM tags WHERE tag_name = '{tagName}';";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.TagDeleted, $"Deleted tag: {tagName}");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.TagDeleteFailed, $"Failed to delete tag: {tagName}");
            }
            return success;
        }

        public static bool DeleteImageTagRelation(string imageName, string tagName)
        {
            var sql = $"DELETE FROM imagetags WHERE image_name = '{imageName}' AND tag_name = '{tagName}';";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.ImageTagRelationDeleted, $"Deleted image-tag relation: {imageName}, {tagName}");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.ImageTagRelationDeleteFailed, $"Failed to delete image-tag relation: {imageName}, {tagName}");
            }
            return success;
        }

        // Logs aren't essential anyway, so the user should have the option to quickly clear them out if the database gets too big
        public static bool DeleteAllLogs()
        {
            var sql = @"DELETE FROM logs;";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.LogsDeleted, "Deleted all logs");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.LogsDeleteFailed, "Failed to delete all logs");
            }
            return success;
        }

        public static bool DeleteLogsForDate(DateTime dateTime)
        {
            var sql = $"DELETE FROM logs WHERE date(datetime) = '{dateTime.ToString("yyyy-MM-dd")}';";

            bool success = _sqliteHelper.ExecuteNonQuery(sql);
            if (success)
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Info, LogEvent.LogsDeleted, $"Deleted logs for {dateTime.ToString("yyyy-MM-dd")}");
            }
            else
            {
                Log.LogToDb(DateTime.UtcNow, LogLevel.Error, LogEvent.LogsDeleteFailed, $"Failed to delete logs for {dateTime.ToString("yyyy-MM-dd")}");
            }
            return success;
        }

        #endregion
    }
}
