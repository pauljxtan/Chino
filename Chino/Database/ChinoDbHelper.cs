using Chino.Model;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace Chino.Database
{
    public static class ChinoDbHelper
    {
        private static SqliteHelper _sqliteHelper = new SqliteHelper(Config.DatabasePath);

        public static List<Image> GetAllImages()
        {
            var images = new List<Image>();

            var sql = @"SELECT image_name FROM images;";

            using (SQLiteDataReader reader = _sqliteHelper.ExecuteQuery(sql))
            {
                while (reader.Read())
                {
                    var imageName = reader["image_name"].ToString();
                    var image = new Image(imageName, GetTagsByImageName(imageName));
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

        public static bool InsertImage(string imageName)
        {
            if (ImageExists(imageName)) return true;
            var sql = $"INSERT INTO images (image_name) VALUES ('{imageName}');";

            return _sqliteHelper.ExecuteNonQuery(sql);
        }

        public static bool RemoveImage(string imageName)
        {
            if (!ImageExists(imageName)) return true;
            var sql = $"DELETE FROM images WHERE image_name = '{imageName}';";

            return _sqliteHelper.ExecuteNonQuery(sql);
        }

        public static bool InsertTag(string tagName)
        {
            if (TagExists(tagName)) return true;
            var sql = $"INSERT INTO tags (tag_name) VALUES ('{tagName}');";

            return _sqliteHelper.ExecuteNonQuery(sql);
        }

        public static bool RemoveTag(string tagName)
        {
            if (!TagExists(tagName)) return true;
            var sql = $"DELETE FROM tags WHERE tag_name = '{tagName}';";

            return _sqliteHelper.ExecuteNonQuery(sql);
        }

        public static bool AddImageTagRelation(string imageName, string tagName)
        {
            var sql = $"INSERT INTO imagetags (image_name, tag_name) VALUES ('{imageName}', '{tagName}');";

            return _sqliteHelper.ExecuteNonQuery(sql);
        }

        public static bool RemoveImageTagRelation(string imageName, string tagName)
        {
            var sql = $"DELETE FROM imagetags WHERE image_name = '{imageName}' AND tag_name = '{tagName}';";

            return _sqliteHelper.ExecuteNonQuery(sql);
        }

        private static List<string> GetTagsByImageName(string imageName)
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
    }
}
