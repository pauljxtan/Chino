using System;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace Chino.Database
{
    public class SqliteHelper : IDisposable
    {
        private SQLiteConnection _dbConnection;

        public SqliteHelper(string databasePath)
        {
            var connectionString = $"Data Source={databasePath};Version=3";
            var initDb = false;

            if (!File.Exists(databasePath))
            {
                SQLiteConnection.CreateFile(databasePath);
                initDb = true;
            }

            _dbConnection = new SQLiteConnection(connectionString);

            if (initDb) CreateTables();
        }


        public bool ExecuteNonQuery(string sql)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
            var command = new SQLiteCommand(sql, _dbConnection);
            int result = command.ExecuteNonQuery();
            return result >= 0;
        }

        public SQLiteDataReader ExecuteQuery(string sql)
        {
            if (_dbConnection.State != ConnectionState.Open)
                _dbConnection.Open();
            var command = new SQLiteCommand(sql, _dbConnection);
            var reader = command.ExecuteReader();
            return reader;
        }

        public void Dispose()
        {
            _dbConnection.Dispose();
        }

        private void CreateTables()
        {
            ExecuteNonQuery(@"
                    CREATE TABLE IF NOT EXISTS images (
                        image_name TEXT PRIMARY KEY
                    );

                    CREATE TABLE IF NOT EXISTS tags (
                        tag_name TEXT PRIMARY KEY
                    );

                    CREATE TABLE IF NOT EXISTS imagetags (
                        image_name TEXT NOT NULL,
                        tag_name TEXT NOT NULL,
                        FOREIGN KEY(image_name) REFERENCES image(image_name),
                        FOREIGN KEY(tag_name) REFERENCES tags(tag_name)
                    );

                    CREATE TABLE IF NOT EXISTS logs (
                        datetime TEXT NOT NULL,
                        log_level TEXT NOT NULL,
                        log_event TEXT NOT NULL,
                        message TEXT
                    );");
        }
    }
}
