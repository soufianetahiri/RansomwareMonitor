using log4net;
using log4net.Config;
using log4net.Repository;
using Microsoft.Extensions.Configuration;
using RansomWatcherV2.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;


namespace RansomWatcherV2
{
    public partial class Form1 : Form
    {
        private IConfiguration _config;
        private Dictionary<string, string> GangList;
        private int counter = 60 * 30 * 1000;
        readonly System.Timers.Timer tR;
        private static  ILog log;


        public Form1()
        {
            // Load configuration
            log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
            InitializeComponent();
            tR = new System.Timers.Timer(1000);
        }
        private bool DownloadDb()
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://xxxxxxxxx/history.db",
                                        Consts.HistoryTemp);
                    if (File.Exists(Consts.Historydb))
                    {
                        if (File.Exists(Consts.HistoryTemp))
                        {
                            if (new FileInfo(Consts.HistoryTemp).Length > new FileInfo(Consts.Historydb).Length)
                            {
                                File.Replace(Consts.HistoryTemp, Consts.Historydb, null, true);
                            }

                        }
                    }
                    else if (File.Exists(Consts.HistoryTemp))
                    {
                        File.Move(Consts.HistoryTemp, Consts.Historydb);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
        }
        private void UpdateSettings()
        {
            try
            {
                using WebClient client = new WebClient();
                statusLabel.Text = "Downloading ransome wiki...";
                client.DownloadFile("https://xxxxxxxxx/appsettings.json",
                                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "appsettings.json");
                _config = new ConfigurationBuilder().SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json").Build();
                GangList = _config.GetSection("Gangs").GetChildren().ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception ex)
            { log.Error(ex?.Message + Environment.NewLine + ex?.InnerException?.Message); }
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            await webView2Control.EnsureCoreWebView2Async();
            statusLabel.Text = "Downloading database...";
            if (DownloadDb())
            {
                UpdateSettings();
                statusLabel.Text = "Database updated";
                if (!string.IsNullOrEmpty(_config.GetSection("ChatId").Value) || !string.IsNullOrEmpty(_config.GetSection("TokenId").Value))
                {
                    Consts.ChatId = _config.GetSection("ChatId").Value;
                    Consts.TokenId = _config.GetSection("TokenId").Value;
                }
                else
                {
                    Consts.ChatId = "-x"; //
                    Consts.TokenId = "x:x";

                }
                tR.Elapsed += new ElapsedEventHandler(Tick);
                tR.Start();
                while (true)
                {
                    _ = LoopAsync();
                    await Task.Delay(60 * 30 * 1000);
                    counter = 60 * 30 * 1000;
                }
            }
            else
            {
                statusLabel.Text = "Unable to download database. Aborting...";
                statusLabel.BackColor = Color.Red;
            }

        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            counter -= 1000;
            try
            {
                timerLabel.Text = string.Concat($"Looping trough {GangList.Count} websites. Next loop in ", TimeSpan.FromMilliseconds(counter).ToString(@"hh\:mm\:ss"));
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                log.Error(ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
            }
        }

        private async Task LoopAsync()
        {
            foreach (KeyValuePair<string, string> item in GangList)
            {
                Worker worker = new Worker();
                await worker.ScrapAsync(item.Value, item.Key);
            }
            statusLabel.Text = "Iteration done, Async functions may still running. Waiting for the next iteration.";
            hashLabel.Text = "Done.";
        }


        private void statusStrip1_Click(object sender, EventArgs e)
        {
            ProcessStartInfo psInfo = new ProcessStartInfo
            {
                FileName = "https://t.me/+yGVBkAapBcQ5NDhk",
                UseShellExecute = true
            };
            Process.Start(psInfo);
        }


    }
}
