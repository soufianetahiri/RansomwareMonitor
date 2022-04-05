using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using log4net;
using System.Reflection;

namespace RansomWatcherV2.Helper
{
    public static class Tclient
    {
        static readonly TwitterSupport.Tweet tweetHelper = new TwitterSupport.Tweet();
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task SendAlertAsync(string message)
        {
            try
            {
                var task = Task.Factory.StartNew(() => BroadcastAsync(message), TaskCreationOptions.LongRunning);
                task.Wait();
                await Task.Delay(500);
            }
            catch (Exception ex)
            {
                log.Error($"{message}" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
            }

        }

        private static async Task<string> BroadcastAsync(string message)
        {
            int sub = 40;
            string prevtxt = Program.mainForm.hashLabel.Text;
        retry:
            try
            {
                message = Regex.Replace(message, @"<figure.+?figure>", "");
                if (message.Contains("Ragnar_Locker"))
                {
                    sub = 30;
                }
                if (message.Contains("Moses Staff"))
                {
                    sub = message.Length - 5;
                }
                string messagehash = Md5(message.Replace(" ", string.Empty).Replace("<b>", string.Empty).Replace("</b>", string.Empty).Replace("\n", string.Empty).Length > sub ?
                    message.Replace(" ", string.Empty).Replace("<b>", string.Empty).Replace("</b>", string.Empty).Replace("\n", string.Empty).Substring(0, sub) : message, true);
                message = message + ($"\n<b>Detection time:</b>{DateTime.Now}");
                string url = $"https://api.telegram.org/bot{Consts.TokenId}/sendMessage?chat_id={Consts.ChatId}&text={HttpUtility.UrlEncode(HttpUtility.HtmlDecode(message))}&parse_mode=html";
                using (var webClient = new WebClient())
                {
                    if (!await LogHelper.AlertSent(messagehash))
                    {
                        try
                        {
                            Program.mainForm.hashLabel.Text = $"Notifying about {messagehash}";
                            Application.DoEvents();
                        }
                        catch (Exception ex)
                        {
                            log.Error($"Fucn BroadcastAsync \n{message}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);

                        }
                        Mysql.LogtoDb(message);
                        await tweetHelper.TweetAsync(message.Length > 190 ? message.Substring(0, 187).Replace("<b>", "#").Replace("</b>", string.Empty) :
                          message.Replace("<b>", "#").Replace("</b>", string.Empty));
                        await webClient.DownloadStringTaskAsync(url);
                        await Task.Delay(4600);
                    }
                    else
                    {
                        try
                        {
                            Program.mainForm.hashLabel.Text = $"The notif id {messagehash} is already sent.";
                            Application.DoEvents();
                        }
                        catch (Exception ex)
                        {
                            log.Error($"Fucn BroadcastAsync \n{message}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);

                        }

                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                log.Error($"Fucn BroadcastAsync \n{message}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);


                if (ex.Message.Contains("429"))
                {
                    Program.mainForm.hashLabel.Text = "Sleeping (too many requests)";
                    Application.DoEvents();
                    Thread.Sleep(20000);
                    Program.mainForm.hashLabel.Text = prevtxt;
                    Application.DoEvents();
                }
                else if (ex.Message.Contains("database is locked"))
                {
                    await Task.Delay(500);
                    goto retry;
                }

                return "";
            }
        }

        private static string Md5(string input, bool isLowercase = false)
        {
            using (var md5 = MD5.Create())
            {
                var byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                var hash = BitConverter.ToString(byteHash).Replace("-", "");
                return (isLowercase) ? hash.ToLower() : hash;
            }
        }
    }
}
