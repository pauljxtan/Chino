using System;
using System.Data;
using System.Data.SQLite;

namespace Chino.Database
{
    public class SqliteHelper : IDisposable
    {
        private SQLiteConnection _dbConnection;

        public SqliteHelper(string databasePath)
        {
            var connectionString = $"Data Source={databasePath};Version=3";
            _dbConnection = new SQLiteConnection(connectionString);
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
    }
}
