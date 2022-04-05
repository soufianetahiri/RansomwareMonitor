using log4net;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace RansomWatcherV2
{
   public static class Mysql
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public static void LogtoDb(string message)
        {
            string connetionString = "server=127.0.0.1;port=5522;database=*;uid=*;*";

            using (MySqlConnection cn = new MySqlConnection(connetionString))
            {
                try
                {
                    string query = "INSERT INTO leaks(leak) VALUES (?leak);";
                    cn.Open();
                    using MySqlCommand cmd = new MySqlCommand(query, cn);
                    cmd.Parameters.Add("?leak", MySqlDbType.LongText).Value = message;
                    cmd.ExecuteNonQuery();
                }
                catch (MySqlException ex)
                {
                    log.Error("Error in adding mysql row. Error: " + ex.Message + Environment.NewLine + message);
                }
            }
        }
    }
}
