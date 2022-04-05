using System;
using System.Data.SQLite;

namespace RansomWatcherV2
{
    public class ConnectionManager
    {
        public int BusyTimeout { get; set; }
        public static ConnectionManager Instance
        {
            get
            {
                if (iInstance == null)
                {
                    lock (instanceLock)
                    {
                        if (iInstance == null)
                            iInstance = new ConnectionManager();
                    }
                }
                return iInstance;
            }
        }
        private static ConnectionManager iInstance = null;

        private static object instanceLock;

        private ConnectionManager()
        {
            BusyTimeout = Convert.ToInt32(TimeSpan.FromMinutes(2).TotalMilliseconds);
        }

        static ConnectionManager()
        {
            instanceLock = new object();
        }

        public SQLiteConnection CreateConnection(string connectionString)
        {
            SQLiteConnection connection = new SQLiteConnection(connectionString);
            connection.Open();

            using (SQLiteCommand command = connection.CreateCommand())
            {
                command.CommandText = string.Format("PRAGMA busy_timeout={0}", BusyTimeout);
                command.ExecuteNonQuery();
            }
            return connection;
        }
    }
}
