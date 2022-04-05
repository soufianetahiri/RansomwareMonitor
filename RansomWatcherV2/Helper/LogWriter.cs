using log4net;
using System;
using System.Data.SQLite;
using System.Reflection;
using System.Threading.Tasks;

namespace RansomWatcherV2.Helper
{
    public static class LogHelper
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static async Task<bool> AlertSent(string hash)
        {
            try
            {
                var connection1 = new SQLiteConnection($"Data Source={Consts.Historydb}");
                connection1.Open();
                SQLiteCommand select = connection1.CreateCommand();
                select.CommandText = @"SELECT id FROM MessageHash WHERE id = '" + hash + "'";
                SQLiteDataReader reader = (SQLiteDataReader)await select.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    reader.Close();
                    await reader.DisposeAsync();
                    connection1.Close();
                    connection1.Dispose();
                    return true;
                }
                else
                {
                    var connection2 = new SQLiteConnection($"Data Source={Consts.Historydb}");
                    connection2.Open();
                    SQLiteCommand insert = connection2.CreateCommand();
                    insert.CommandText = @"INSERT INTO MessageHash" + "(id)" + "Values('" + hash + "') ";
                    _ = await insert.ExecuteNonQueryAsync();
                    connection2.Close();
                    connection2.Dispose();
                    return false;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
        }
    }
}
