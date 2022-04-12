
using HtmlAgilityPack;
using log4net;
using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using RansomWatcherV2.GenericModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml.Linq;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace RansomWatcherV2.Helper
{
    public class Worker
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        static readonly HtmlWeb web = new HtmlWeb();
        public Worker()
        {
        }

        public async Task ScrapAsync(string url, string groupename)
        {
            if (groupename == "Conti")
            {
                NavigateAndRefreshGui(url);
                UpdateStatus(groupename);
                HtmlDocument doc = await web.LoadFromWebAsync(url);
                var scripts = doc.DocumentNode.Descendants("script");
                foreach (HtmlNode script in scripts)
                {
                    try
                    {
                        if ((bool)(script?.InnerHtml?.Contains("newsList([{")))
                        {
                            var tmp = "[" + script.InnerHtml.TrimStart().TrimEnd().Replace("newsList([", string.Empty).Replace("]);", string.Empty) + "]";
                            List<ContiLeak> data = JsonConvert.DeserializeObject<List<ContiLeak>>(tmp);
                            if (data?.Count > 0)
                            {
                                foreach (ContiLeak leak in data)
                                {
                                    var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{leak.title} \n\n{leak.links} \n\n{leak.about} \n\n<b>Address:</b>\n{leak.address} \n\n" +
                                        $"<b>Publish Date:</b>\n{DateTimeOffset.FromUnixTimeSeconds(leak.date).DateTime.ToShortDateString()}\n\n<b>Published {leak.published}% of data.\n Seen {leak.view} times.</b>"), TaskCreationOptions.LongRunning);
                                    task.Wait();
                                    await Task.Delay(300);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                    }
                }
            }
            if (groupename == "Everest" || groupename == "54bb47h" || groupename == "Arvin Leaks" || groupename == "Cuba")
            {
                try
                {
                    NavigateAndRefreshGui(url);
                    UpdateStatus(groupename);
                    IEnumerable<BlogItem> blogs = RssHelper.ReadFeed(url);
                    if (blogs != null && blogs.Count() > 0)
                    {
                        foreach (BlogItem item in blogs)
                        {
                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{item.Title} \n\n{item.Summary} \n\n<b>Publish Date:</b>\n{item.PublishDate}"),
                                TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(300);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Ragnar_Locker")
            {

                NavigateAndRefreshGui(url);
                UpdateStatus(groupename);
                HtmlDocument doc = await web.LoadFromWebAsync(url);
                var script = doc.DocumentNode.SelectSingleNode(Consts.ragnarXpath)?.InnerText;
                string extract = Between(script, "var post_links =", " ;");
                try
                {
                    List<Ragnar> data = JsonConvert.DeserializeObject<List<Ragnar>>(extract);
                    if (data?.Count > 0)
                    {
                        foreach (Ragnar leak in data)
                        {
                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{leak.title}\n{leak.info.TrimEnd()}\n\n" +
                                $"<b>Publish Date:</b>\n{DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(leak.timestamp))} \n\n<b>Leak seen:</b>\n{leak.views} times"), TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(300);
                        }
                    }

                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Grief")
            {
                NavigateAndRefreshGui(url);
                UpdateStatus(groupename);
                using (var webClient = new WebClient())
                {
                    var i = 0;
                retry:
                    webClient.Headers.Add("user-agent", RandomUA());

                    try
                    {
                        string json = await webClient.DownloadStringTaskAsync(url);
                        Grief data = JsonConvert.DeserializeObject<Grief>(json);
                        if (data?.data?.Count > 0)
                        {
                            foreach (Datum leak in data.data)
                            {
                                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{leak.title} \n\n{leak.url} \n\n<b>Leak seen:</b>" +
                                    $"\n{leak.views} times \n\n<b>Publish Date:</b>\n{leak.published_at} \n\n <b>Update Date:</b>\n{leak.updated_at}"), TaskCreationOptions.LongRunning);
                                task.Wait();
                                await Task.Delay(300);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                        if (ex.Message.Contains("502"))
                        {
                            if (i < 5)
                            {
                                i++;
                                goto retry;
                            }

                        }
                    }
                }
            }
            if (groupename == "Pysa")
            {
                NavigateAndRefreshGui(url);
                HtmlDocument doc = await web.LoadFromWebAsync(url);
                var blocks = doc.DocumentNode.SelectNodes(Consts.pysaLeakblock);
                UpdateStatus(groupename);
                if (blocks != null)
                {
                    foreach (HtmlNode leak in blocks)
                    {
                        try
                        {
                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{leak.ChildNodes[1].InnerText} \n\n{leak.ChildNodes[6].InnerText} \n\n " +
                                $"<b>Publish Date:</b>\n{leak.ChildNodes[3].InnerText}"), TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(300);
                        }
                        catch (Exception ex)
                        {
                            log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                        }
                    }

                }
            }
            if (groupename == "RansomEXX")
            {
                NavigateAndRefreshGui(url);
                try
                {
                    UpdateStatus(groupename);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes(Consts.ransomExxLeakkblock);
                    foreach (HtmlNode leak in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(leak.InnerHtml);
                        string name = htmlDoc.DocumentNode.SelectSingleNode("//h5[contains(@class, 'card-title')]")?.InnerText;
                        string urlv = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'card-text')][1]")?.InnerText;
                        string desc = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'card-text')][2]")?.InnerText;
                        string publish = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'card-text mt-3 text-secondary')]")?.InnerText;
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{HttpUtility.HtmlDecode(name)} \n\n{HttpUtility.HtmlDecode(urlv)} \n\n<b>About the victim:</b>\n" +
                            $"{HttpUtility.HtmlDecode(desc)}\n<b>Publish Date:</b>\n{publish.Split(",")[0]}\n<b>Leak seen:</b>\n{publish.Split(",")[1]}\n<b>Leak size:</b>\n{publish.Split(",")[2]}"), TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(300);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Xing Locker")
            {
                NavigateAndRefreshGui(url);
                try
                {
                    UpdateStatus(groupename);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes(Consts.XingBlocks);
                    foreach (HtmlNode htmlNode in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlNode.InnerHtml);
                        string views = htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'float-end')]")?.InnerText.TrimEnd();
                        string publish = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'mb-1 text-muted')]")?.InnerText.Replace("\n", string.Empty).TrimStart().TrimEnd();
                        string desc = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'card-text mb-auto')]")?.InnerText.Replace("\n", string.Empty).TrimStart().TrimEnd();
                        string name = htmlDoc.DocumentNode.SelectSingleNode("//h3[contains(@class, 'mb-0')]")?.InnerText.Replace(views, string.Empty).Replace("\n", string.Empty).TrimEnd().TrimStart();
                        views = views.Replace("\n", string.Empty).TrimStart();
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{HttpUtility.HtmlDecode(name)} \n\n<b>About the victim:</b>\n{HttpUtility.HtmlDecode(desc)}\n\n" +
                            $"<b>Publish Date:</b>\n{IsNA(publish)}\n\n<b>Leak seen:</b>\n{IsNA(views)}"), TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(300);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Marketo")
            {
                try
                {
                    UpdateStatus(groupename);
                    bool keep = true;
                    int i = 1;
                    while (keep)
                    {
                        keep = await NavigateMerkatoAsync(string.Format(url, i), groupename);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Atomsilo")
            {
                try
                {
                    NavigateAndRefreshGui(url);
                    UpdateStatus(groupename);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes(Consts.AtomSiloBlocks);
                    foreach (HtmlNode htmlNode in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlNode.InnerHtml);
                        string leaksize = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'd-flex font-weight-bold')]")?.InnerText.TrimEnd().TrimStart().Replace("\r\n", string.Empty).Replace("                                                  ", " ");
                        string desc = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'card-body post-body-preview p-3')]")?.InnerText.Replace("\r\n                                ", string.Empty);
                        string website = htmlDoc.DocumentNode.SelectSingleNode("//a[contains(@class, 'btn btn-outline-light btn-sm p-1 btn-site')]")?.InnerText.TrimStart().TrimEnd();
                        string name = htmlDoc.DocumentNode.SelectSingleNode("//h4[contains(@class, 'post-announce-name')]")?.InnerText;
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{HttpUtility.HtmlDecode(name)}\n\n<b>Website:</b>{IsNA(website)} \n\n<b>About the victim:</b>\n{HttpUtility.HtmlDecode(desc)}\n\n" +
                            $"<b>{IsNA(leaksize)}</b>"), TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(300);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Hive Leaks")
            {
                try
                {
                    UpdateStatus(groupename);
                    NavigateAndRefreshGui(url + "v1/links/disclosed");
                    HtmlDocument doc = await web.LoadFromWebAsync(url + "v1/links/disclosed");
                    List<HiveLeak> data = JsonConvert.DeserializeObject<List<HiveLeak>>(doc.DocumentNode.InnerText);

                    HtmlDocument companies = await web.LoadFromWebAsync(url + "v1/companies/disclosed");
                    List<HiveLeakCompanies> companiesdata = JsonConvert.DeserializeObject<List<HiveLeakCompanies>>(companies.DocumentNode.InnerText);
                    foreach (HiveLeakCompanies companieLeaked in companiesdata)
                    {
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{VoidOrValue(companieLeaked.title)}\n<b>Info:</b>\n{VoidOrValue(companieLeaked.website)}\n{VoidOrValue(companieLeaked.description)}\n" +
                            $"<b>Url: </b>{VoidOrValue(companieLeaked.website)}\n<b>Revenue ($): </b>{VoidOrValue(companieLeaked.revenue)} \n<b>Employees: </b>{VoidOrValue(companieLeaked.employees)}\n" +
                     $"<b>Encrypted At: </b>{VoidOrValue(companieLeaked.encrypted_at)} \n<b>Disclosed At: </b>{VoidOrValue(companieLeaked.disclosed_at)}"), TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(300);
                    }

                    foreach (HiveLeak leak in data)
                    {
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n<b>Info</b>\n{VoidOrValue(leak.title)}\n{VoidOrValue(leak.description)}\n<b>Url:</b>{VoidOrValue(leak.url)}" +
                            $" \n<b>Password</b>\n {VoidOrValue(leak.password)} \n<b>Leak date:</b>\n{HttpUtility.HtmlDecode(leak.created_at?.ToString())}\n" +
                            $"<b>Leak size (Gb):</b>\n{ToSize(leak.size, SizeUnits.GB)}"), TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(300);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Quantum Blog")
            {
                try
                {
                    NavigateAndRefreshGui(url);
                    UpdateStatus(groupename);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//section[contains(@class, 'blog-post')]");
                    foreach (HtmlNode htmlNode in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlNode.InnerHtml);
                        string leakdate = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'blog-post-date pull-right')]")?.InnerText.TrimEnd().TrimStart();
                        string leaksize = htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'label label-light label-info')]")?.InnerText.TrimEnd().TrimStart();
                        string leakviews = htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'btn btn-success pull-right')]")?.InnerText.Replace("\n                  visibility\n             ", string.Empty);
                        string desc = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'blog-post-content')]/p")?.InnerText.TrimStart().TrimEnd();
                        string name = htmlDoc.DocumentNode.SelectSingleNode("//h2[contains(@class, 'blog-post-title')]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{HttpUtility.HtmlDecode(name)}\n\n<b>About the victim:</b>\n{HttpUtility.HtmlDecode(desc)}\n\n<b>Publish Date:</b>{(!string.IsNullOrEmpty(leakdate) ? leakdate : Consts.NA)} \n<b>Leak seen:</b>\n{IsNA(leakviews)} times \n<b>Leak size:</b>{IsNA(leaksize)} \n" +
                            $"<b>{IsNA(leakdate)}</b>"), TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(300);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "CL0P")
            {
                try
                {
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    UpdateStatus(groupename);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//div[contains(@class, 'notices blue')]");
                    foreach (HtmlNode htmlNode in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlNode.InnerHtml);
                        string[] Leaks = htmlDoc.DocumentNode.SelectSingleNode("//p")?.InnerText.Split("\n");
                        for (int i = 1; i < Leaks.Length; i++)
                        {
                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}</b> Added new leak:\n {Leaks[i]}"), TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(300);
                        }
                        await Task.Delay(500);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }

            }
            if (groupename == "AlphVM")
            {
                try
                {
                    UpdateStatus(groupename);
                    bool keep = true;
                    int i = 1;
                    while (keep)
                    {
                        keep = await NavigateAlphVMsync(string.Format(url, i), groupename);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Lorenz")
            {
                try
                {
                    UpdateStatus(groupename);
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//div[contains(@class, 'panel panel-primary')]");
                    if (blocks != null)
                    {
                        foreach (HtmlNode htmlNode in blocks)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlNode.InnerHtml);
                            HtmlNode Leaks = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'panel-heading')]");
                            string name = string.Empty;
                            string progress = string.Empty;
                            string leakdate = string.Empty;
                            string website = string.Empty;

                            if (Leaks != null)
                            {
                                name = Leaks.SelectSingleNode("//h3")?.InnerText;
                                leakdate = Leaks.SelectSingleNode("//h5[1]")?.InnerText;
                                website = Leaks.SelectSingleNode("//h5[2]")?.InnerText;
                            }
                            //    try to get progress if present
                            try
                            {
                                progress = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'progress-bar progress-bar-striped active')]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                            }
                            catch (Exception ex)
                            {
                                log.Error($"{groupename}\n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                            }
                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{IsNA(name)}\n<b>Website:</b>\n{IsNA(website)}\n<b>Leak Date:</b>\n" +
                                $"{(!string.IsNullOrEmpty(leakdate) ? leakdate : Consts.NA)}\n<b>{IsNA(progress)}</b>"),
                                TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Payload.bin")
            {
                try
                {
                    UpdateStatus(groupename);
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//div[contains(@class, 'disclosed__inner')]/div[contains(@class, 'tab--news')]");
                    if (blocks != null)
                    {
                        foreach (HtmlNode htmlNode in blocks)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlNode.InnerHtml);
                            string Details = string.Empty;
                            string Website = string.Empty;
                            string LeakDate = string.Empty;
                            string Views = string.Empty;

                            Website = htmlDoc.DocumentNode.SelectSingleNode("//h4")?.InnerText;
                            Details = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'simple-text nonactive')]")?.InnerText;
                            Views = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'info--text views')]")?.InnerText;
                            LeakDate = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'info--text data')]")?.InnerText;

                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n<b>website:</b>\n{IsNA(Website)}\n<b>About the leak:</b>\n" +
                                $"{IsNA(Details)}\n<b>Leak seen {IsNA(Views)} times</b>\n<b>Publish Date:</b>\n{IsNA(LeakDate)}"),
                                       TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(500);

                        }
                    }
                }

                catch (Exception ex)
                {
                    log.Error("Payload.bin\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "LockData Auction")
            {
                try
                {
                    UpdateStatus(groupename);
                    bool keep = true;
                    int i = 1;
                    while (keep)
                    {
                        keep = await NavigateToLockData(string.Format(url, i), groupename);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    log.Error("LockData Auction\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Moses Staff")
            {
                try
                {
                    UpdateStatus(groupename);
                    NavigateAndRefreshGui(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//div[(@class='elementskit-post-image-card')]");
                    if (blocks != null)
                    {
                        string name;
                        foreach (HtmlNode htmlNode in blocks)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlNode.InnerHtml);
                            name = htmlDoc.DocumentNode.SelectSingleNode("//h2")?.InnerText.Replace("\n", string.Empty).TrimStart().TrimEnd();
                            if (!name.Contains("This is just the beginning"))
                            {
                                string logo;
                                logo = htmlDoc.DocumentNode.SelectSingleNode("//img").Attributes["src"].Value;
                                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<a href='{IsNA(logo)}'> </a>\n<b>{IsNA(groupename)} </b> breached {IsNA(name)} "),
                                           TaskCreationOptions.LongRunning);
                                task.Wait();
                                await Task.Delay(500);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Moses Stafft\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "DarkLeak Market")
            {
                bool keep = true;
                try
                {
                    UpdateStatus(groupename);
                    int i = 1;
                    while (keep)
                    {
                        keep = await NavigateToDarkLeakMarket(string.Format(url, i), groupename);
                        i++;
                    }
                }
                catch (Exception ex)
                {
                    keep = false;
                    log.Error("DarkLeak Market\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "LV")
            {
                NavigateAndRefreshGui(url);
                UpdateStatus(groupename);
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", RandomUA());
                    string json = await webClient.DownloadStringTaskAsync(url);
                    try
                    {
                        LV data = JsonConvert.DeserializeObject<LV>(json);
                        await LVPostLoop(groupename, data);
                        for (int i = 2; i < data.pages_count; i++)
                        {
                            using (WebClient c = new WebClient())
                            {
                                webClient.Headers.Add("user-agent", RandomUA());
                                NavigateAndRefreshGui(url.Replace("/1", $"/{i}"));
                                string tmp = await webClient.DownloadStringTaskAsync(url.Replace("/1", $"/{i}"));
                                LV tmpj = JsonConvert.DeserializeObject<LV>(json);
                                await LVPostLoop(groupename, tmpj);
                            }
                            await Task.Delay(300);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("LV\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                    }
                }
            }
            if (groupename == "HARON Ransomware2")
            {
                try
                {
                    UpdateStatus(groupename);
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//div[contains(@class, 'col-lg-4 border border-primary blog-post')]");
                    if (blocks != null)
                    {
                        foreach (HtmlNode htmlNode in blocks)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlNode.InnerHtml);
                            HtmlNode Leaks = htmlDoc.DocumentNode.SelectSingleNode("//table[contains(@class, 'table table-sm')]");
                            string Company = string.Empty;
                            string Address = string.Empty;
                            string Website = string.Empty;
                            string Phone = string.Empty;
                            string Views = string.Empty;

                            if (Leaks != null)
                            {
                                Company = Leaks.SelectSingleNode("//td[1]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                                Address = Leaks.SelectSingleNode("//tr[2]/td")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                                Website = Leaks.SelectSingleNode("//tr[3]/td")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                                Phone = Leaks.SelectSingleNode("//tr[4]/td")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                            }

                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{IsNA(Company)}\n<b>Website:</b>\n{IsNA(Website)}\n<b>Address:</b>\n" +
                                $"{IsNA(Address)}\n<b>Phone:</b>\n{IsNA(Phone)}\n"),
                                TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("HARON Ransomware2\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Vice Society")
            {
                try
                {
                    UpdateStatus(groupename);
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectSingleNode("//table[1]//table[1]")?.SelectNodes("//tr");
                    if (blocks != null)
                    {
                        for (int i = 4; i < blocks.Count; i += 2)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(blocks[i].InnerHtml);
                            string name = htmlDoc.DocumentNode.SelectSingleNode("//font[1]//b")?.InnerText;
                            string website = htmlDoc.DocumentNode.SelectSingleNode("//a//font//b")?.InnerText;
                            string country = htmlDoc.DocumentNode.SelectSingleNode("//font[2]")?.InnerText;
                            string desc = htmlDoc.DocumentNode.SelectSingleNode("//font[3]//b")?.InnerText;
                            string logo = htmlDoc.DocumentNode.SelectSingleNode("//img")?.Attributes["src"].Value; //<a href='{logo}'> </a>\n

                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{IsNA(name)}\n<b>Website:</b>\n{IsNA(website)}\n<b>Country:</b>\n" +
                                $"{IsNA(country)}\n<b>About the victime :</b>\n{IsNA(desc)}\n"),
                                TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Vice Society\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Night Sky")
            {
                try
                {
                    UpdateStatus(groupename);
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//div[contains(@class, 'mdui-card-primary-title')]/a");
                    if (blocks?.Count > 0)
                    {
                        foreach (HtmlNode htmlNode in blocks)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlNode.InnerHtml);
                            string link = htmlNode.Attributes["href"]?.Value;
                            if (!string.IsNullOrEmpty(link) && !string.IsNullOrWhiteSpace(link))
                            {
                                link = "http:" + link;
                                Program.mainForm.webView2Control.CoreWebView2.Navigate(link);
                                HtmlDocument leak = await web.LoadFromWebAsync(link);
                                string name = leak.DocumentNode.SelectSingleNode("//div[contains(@class, 'mdui-card-primary-title')]")?.InnerText;
                                string leakdate = leak.DocumentNode.SelectSingleNode("//div[contains(@class, 'mdui-card-primary-subtitle')]")?.InnerText;
                                string steallist = leak.DocumentNode.SelectSingleNode("//div/ul[1]")?.InnerText;

                                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{IsNA(name)}\n<b>About the leak:</b>\n" +
                                    $"{IsNA(steallist)}\n<b>{IsNA(leakdate)}</b>"),
                                    TaskCreationOptions.LongRunning);
                                task.Wait();
                                await Task.Delay(500);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {

                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);

                }
            }
            if (groupename == "Snatch")
            {
                NavigateAndRefreshGui(url);
                UpdateStatus(groupename);
                try
                {
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes(Consts.snatchLeakkblock);
                    foreach (HtmlNode leak in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(leak.InnerHtml);
                        string name = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'a-b-n-name')]")?.InnerText;
                        string dataadded = htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'a-b-n-tag')]")?.InnerText;
                        string desc = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'a-b-text')]")?.InnerText;
                        string publish = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'a-b-h-time')]")?.InnerText;
                        string views = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'a-b-h-v-c-views')]")?.InnerText;
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{HttpUtility.HtmlDecode(name)} \n\n<b>About the victim:</b>\n{HttpUtility.HtmlDecode(desc)}\n" +
                            $"<b>Publish Date:</b>\n{IsNA(publish)}\n<b>Leak seen:</b>\n{IsNA(views)}\n<b>Leak size:</b>\n{IsNA(dataadded)}"), TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(300);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "Suncrypt")
            {
                try
                {
                    UpdateStatus(groupename);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    HtmlDocument pagesN = new HtmlDocument();
                    var blocks = doc.DocumentNode.SelectNodes("//nav[contains(@class, 'pagination')]");
                    pagesN.LoadHtml(blocks?[0].InnerHtml);
                    int? pages = pagesN.DocumentNode.SelectNodes("//li")?.Count();
                    string suffixpage = "?p={0}";
                    List<string> urlsList = new List<string>();
                    if (pages != null && pages > 0)
                    {
                        for (int i = 0; i < pages; i++)
                        {
                            doc = await web.LoadFromWebAsync(string.Format(url + suffixpage, i));
                            var u = doc.DocumentNode.SelectNodes("//p[contains(@class, 'title is-5 mt-5')]//a");
                            foreach (HtmlNode item in u)
                            {
                                urlsList.Add(item.GetAttributeValue("href", string.Empty));
                            }
                        }
                    }
                    if (urlsList.Count > 0)
                    {
                        foreach (string victim in urlsList)
                        {
                            if (!string.IsNullOrEmpty(victim))
                            {
                                await NavigateToSunCrypt("http:" + victim, groupename);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "CRYP70N1C0D3")
            {
                try
                {
                    UpdateStatus(groupename);
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//article[contains(@class, 'left_article')]");
                    if (blocks != null)
                    {
                        foreach (HtmlNode htmlNode in blocks)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlNode.InnerHtml);
                            HtmlNode Leaks = htmlDoc.DocumentNode.SelectSingleNode("//tbody[1]");
                            string Site = string.Empty;
                            string Status = string.Empty;
                            string Price = string.Empty;

                            if (Leaks != null)
                            {
                                var nodes = Leaks.SelectNodes("//tr");
                                if (nodes != null && nodes.Count > 0)
                                {
                                    for (int i = 1; i < nodes.Count; i++)
                                    {
                                        HtmlDocument htmlDocnodes = new HtmlDocument();
                                        htmlDocnodes.LoadHtml(nodes[i].InnerHtml);
                                        Site = htmlDocnodes.DocumentNode.SelectSingleNode("//td[1]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                                        Status = htmlDocnodes.DocumentNode.SelectSingleNode("//td[2]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                                        Price = htmlDocnodes.DocumentNode.SelectSingleNode("//td[3]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                                        if (Status.Contains("OPEN", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            Status = "Available for sell more than 1 time";
                                        }
                                        if (Status.Contains("CLOSE", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            Status = "Sold";
                                        }
                                        if (Status.Contains("VIP", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            Status = "Available for 1 time only sell";
                                        }
                                        if (Status.Contains("BID", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            Status = "Available for the best bid";
                                        }
                                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\nDatabase from <b>{IsNA(Site)}</b> is <b>{IsNA(Status)}</b>\n<b>Price:</b>" +
                         $"{IsNA(Price)}"),
                         TaskCreationOptions.LongRunning);
                                        task.Wait();
                                        await Task.Delay(500);
                                    }

                                }

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("CRYP70N1C0D3\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
            if (groupename == "LOCKBIT 2.0")
            {
                UpdateStatus(groupename);
                Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                while (!Program.mainForm.webView2Control.CoreWebView2.DocumentTitle.Contains("LockBit"))
                {
                    await Task.Delay(200);
                }
                if (Program.mainForm.webView2Control.CoreWebView2.DocumentTitle.Contains("LockBit"))
                {
                    try
                    {
                        Application.DoEvents();
                        while (Program.mainForm.webView2Control.CoreWebView2.DocumentTitle == "LockBit Anti-DDos protection")
                        {
                            await Task.Delay(8000);
                        }
                        string strResult = await Program.mainForm.webView2Control.CoreWebView2.ExecuteScriptAsync("document.documentElement.innerHTML;");
                        strResult = Regex.Unescape(strResult);
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(strResult);
                        var blocks = htmlDoc.DocumentNode.SelectNodes("//div[contains(@class, 'post-block  bad ')] | //div[contains(@class, 'post-block  good ')]");
                        if (blocks?.Count > 0)
                        {
                            Program.mainForm.webView2Control.Stop();
                            foreach (var leak in blocks)
                            {
                                HtmlDocument tmpDoc = new HtmlDocument();
                                tmpDoc.LoadHtml(leak.InnerHtml);
                                string name = tmpDoc.DocumentNode.SelectSingleNode("//div[@class='post-title']")?.InnerText.TrimStart().TrimEnd();
                                string desc = tmpDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'post-block-text')]")?.InnerText.TrimStart().TrimEnd();
                                string leakdate = string.Empty;
                                try
                                {
                                    string leakIn = tmpDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'post-timer')]")?.Attributes["data-timer"]?.Value.TrimStart().TrimEnd();
                                    leakdate = $"<b>Publish date:</b>\n{(!string.IsNullOrEmpty(leakIn) ? leakIn : Consts.NA)}";
                                }
                                catch (Exception ex)
                                {
                                    log.Error($"{groupename}\n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);

                                }
                                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>LockBit2.0:</b>\n{HttpUtility.HtmlDecode(name)}\n<b>About the victim:</b>\n{HttpUtility.HtmlDecode(IsNA(desc))}\n{IsNA(leakdate)}"), TaskCreationOptions.LongRunning);
                                task.Wait();
                                await Task.Delay(500);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                    }


                }

            }
            if (groupename == "BlackByte")
            {
                try
                {
                    UpdateStatus(groupename);
                    Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                    HtmlDocument doc = await web.LoadFromWebAsync(url);
                    var blocks = doc.DocumentNode.SelectNodes("//div[contains(@class, 'row jumbotron p-3  text-white rounded bg-d1')]");
                    if (blocks?.Count > 0)
                    {
                        foreach (HtmlNode htmlNode in blocks)
                        {
                            HtmlDocument htmlDoc = new HtmlDocument();
                            htmlDoc.LoadHtml(htmlNode.InnerHtml);
                            string name = htmlDoc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'display-4 font-italic border-bottom')]")?.InnerText;
                            string desc = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'lead my-3 pb-3')]")?.InnerText;
                            string employees = htmlDoc.DocumentNode.SelectSingleNode("//span[2]")?.InnerText;
                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{IsNA(name)}\n<b>About the victim:</b>\n" +
                                $"{IsNA(desc)}\n<b>Employees:\n</b>{IsNA(employees)}"),
                                TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(500);
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("BlackByte\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);

                }
            }
            if (groupename == "Pandora Data Leak")
            {
                try
                {
                    UpdateStatus(groupename);
                    NavigateAndRefreshGui(url);
                    XDocument doc = XDocument.Load(url);
                    foreach (var item in doc.Root.Elements().Where(i => i.Name.LocalName == "entry"))
                    {
                        ProcessPandora(groupename, item);
                        await Task.Delay(300);
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"{groupename}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                }
            }
        }
        private static async Task<bool> NavigateToSunCrypt(string url, string groupename)
        {
            try
            {
                NavigateAndRefreshGui(url);

                HtmlDocument doc = await web.LoadFromWebAsync(url);
                if (doc.DocumentNode.InnerText.Contains("Forbidden"))
                {
                    return true;
                }
                var blocks = doc.DocumentNode.SelectNodes("//div[(@class='column')]");
                if (blocks != null && blocks.Count > 0)
                {
                    foreach (HtmlNode htmlNode in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlNode.InnerHtml);

                        string Website = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'subtitle')]")?.InnerText;
                        string Name = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'title is-4')]")?.InnerText;
                        string desc = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'card mb-5')][3]/div[contains(@class, 'card-content')]/div[contains(@class, 'content')]")?.InnerText;

                        string table = htmlDoc.DocumentNode.SelectSingleNode("//table[(@class='table')]").InnerHtml;
                        HtmlDocument tabledoc = new HtmlDocument();
                        tabledoc.LoadHtml(table);
                        string Lockdate = tabledoc.DocumentNode.SelectSingleNode("//tr[1]/td[2]")?.InnerText;
                        string Phone = tabledoc.DocumentNode.SelectSingleNode("//tr[2]/td[2]")?.InnerText;
                        string Address = tabledoc.DocumentNode.SelectSingleNode("//tr[3]/td[2]")?.InnerText;
                        string Fulldump = tabledoc.DocumentNode.SelectSingleNode("//tr[4]/td[2]")?.InnerText;
                        string DDOS = tabledoc.DocumentNode.SelectSingleNode("//tr[5]/td[2]")?.InnerText;


                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}: </b>\n{IsNA(Name)}\n<b>Website: </b>{IsNA(Website)}\n<b>About the victim: </b>\n" +
                            $"{IsNA(desc)}\n<b>Lock date: </b> {IsNA(Lockdate)}\n<b>Phone: </b>{IsNA(Phone)}\n<b>Address: </b>{IsNA(Address)}\n<b>Full Dump: </b>{IsNA(Fulldump)}\n<b>DDOS: </b>{IsNA(DDOS)}"),
                                   TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(500);
                    }
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                log.Error($"{groupename}\n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
            return true;
        }
        private static async Task<bool> NavigateToDarkLeakMarket(string url, string groupename)
        {
            try
            {
                NavigateAndRefreshGui(url);
                int i = 0;
            retry:
                HtmlDocument doc = await web.LoadFromWebAsync(url);
                if ((doc.DocumentNode.InnerText.Contains("Fatal error") || doc.DocumentNode.InnerText.Contains("Tor2Web")) && i < 5)
                {
                    i++;
                    goto retry;
                }
                var blocks = doc.DocumentNode.SelectNodes("//div[(@class='post')]");
                if (blocks != null && blocks.Count > 0)
                {
                    foreach (HtmlNode htmlNode in blocks)
                    {
                        HtmlDocument htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlNode.InnerHtml);

                        string DateAndPrice = htmlDoc.DocumentNode.SelectSingleNode("//p[(@class='details1')]")?.InnerText;
                        string Name = htmlDoc.DocumentNode.SelectSingleNode("//h2")?.InnerText;
                        string Desc = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'content')]/p")?.InnerText;
                        string ViewsSells = htmlDoc.DocumentNode.SelectSingleNode("//p[contains(@class, 'details2')]")?.InnerText;
                        var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)} is selling </b>\n{IsNA(Name)}\n<b>About the data:</b>\n" +
                            $"{(!string.IsNullOrEmpty(Desc) ? Desc : Consts.NA)}\n<b>Date of publication & Price: </b> {IsNA(DateAndPrice)}\n<b>Stats: </b>{IsNA(ViewsSells)}"),
                                   TaskCreationOptions.LongRunning);
                        task.Wait();
                        await Task.Delay(500);
                    }
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                log.Error($"{groupename}\n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
            return true;
        }
        private static async Task<bool> NavigateToLockData(string url, string groupename)
        {
            try
            {
                NavigateAndRefreshGui(url);
                HtmlDocument doc = await web.LoadFromWebAsync(url);
                var blocks = doc.DocumentNode.SelectNodes("//div[(@class='auction-list')]");
                if (blocks != null)
                {
                    foreach (HtmlNode htmlNode in blocks)
                    {
                        var leaks = htmlNode.SelectNodes("//div[(@class='auction-item _leaked')]");
                        if (leaks?.Count > 0)
                        {
                            foreach (HtmlNode node in leaks)
                            {
                                HtmlDocument htmlDoc = new HtmlDocument();
                                htmlDoc.LoadHtml(node.InnerHtml);
                                string Details = string.Empty;
                                string AUctionDetails = string.Empty;
                                string Views = string.Empty;
                                Details = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'auction-item-info__text')]")?.InnerHtml.Replace("<b></b>", string.Empty).Replace("<p>", string.Empty).
                                    Replace("</p>", string.Empty).Replace("<br>", string.Empty).Replace("</b>", "</b> ").TrimStart().TrimEnd();
                                Views = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class,'auction-item-info__views')]")?.InnerText.Replace("\n", string.Empty).Replace("</b><b>", "</b><b> ").Replace("</b>", "</b> ").TrimStart().TrimEnd();
                                AUctionDetails = htmlDoc.DocumentNode.SelectSingleNode("//ul[contains(@class,'auction-item-right__list')]")?.InnerHtml.
                                    Replace("\n", string.Empty).TrimStart().TrimEnd().Replace("span", "b").Replace("<strong>", string.Empty).Replace("</strong>", string.Empty).
                                    Replace("<li>", string.Empty).Replace("</li>", "\n").TrimStart().TrimEnd().Replace("  ", string.Empty).Replace(" <b>", "<b>").Replace("</b>", "</b> ");


                                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}</b>\n<b>About the victim:</b>\n{IsNA(Details)}\n<b>About the auction:</b>\n" +
                                    $"{(!string.IsNullOrEmpty(AUctionDetails) ? AUctionDetails : Consts.NA)}\n<b>Number of {IsNA(Views)}</b>"),
                                           TaskCreationOptions.LongRunning);
                                task.Wait();
                                await Task.Delay(500);
                            }
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }

            catch (Exception ex)
            {
                log.Error($"{groupename}\n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
            return true;
        }
        private static async Task<bool> NavigateAlphVMsync(string url, string groupename)
        {
            NavigateAndRefreshGui(url);
            try
            {
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("user-agent", RandomUA());
                    string json = await webClient.DownloadStringTaskAsync(url);
                    AlphVm data = JsonConvert.DeserializeObject<AlphVm>(json);
                    if (data == null || data.items == null || data.items.Count == 0)
                    {
                        return false;
                    }
                    if (data?.items?.Count > 0)
                    {
                        foreach (Item leak in data.items)
                        {
                            string leakdate = string.Empty;
                            if (!string.IsNullOrEmpty(leak.createdDt.ToString()))
                            {
                                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(leak.createdDt);
                                DateTime dateTime = dateTimeOffset.DateTime;
                                leakdate = dateTime.ToShortDateString();
                            }
                            leak.previewContent = Regex.Replace(leak.previewContent, @"<img.+?>", "");

                            var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{leak.title}\n\n<b>Message from the group:</b>" +
                                $"\n{leak.previewContent.Replace("<p>", string.Empty).Replace("</p>", string.Empty).Replace("<br />", string.Empty).Replace("</br>", string.Empty)} times\n<b>Publish Date:</b>\n{IsNA(leakdate)}"), TaskCreationOptions.LongRunning);
                            task.Wait();
                            await Task.Delay(300);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error($"{groupename}\n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
            return true;
        }
        private static string IsNA(string o)
        {
            if (!string.IsNullOrEmpty(o) && !string.IsNullOrWhiteSpace(o))
            {
                return o.TrimStart().TrimEnd();
            }
            else
            {
                return _ = Consts.NA;
            }
        }
        private static string VoidOrValue(object o)
        {
            if (o != null)
            {
                return o.ToString();
            }
            return _ = Consts.NA;
        }
        public enum SizeUnits
        {
            Byte, KB, MB, GB, TB, PB, EB, ZB, YB
        }
        public static string ToSize(long? value, SizeUnits unit)
        {
            try
            {
                if (value != null)
                {
                    return ((long)value / (double)Math.Pow(1024, (long)unit)).ToString("0.00");
                }
                else
                {
                    return "0";
                }

            }
            catch (Exception ex)
            {
                log.Error($"Func ToSize()\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return "0";
            }
        }
        private static void ProcessRook(string groupename, XElement item)
        {
            var content = item.Elements().First(i => i.Name.LocalName == "content").Value;
            var leakdate = item.Elements().First(i => i.Name.LocalName == "updated").Value;
            if (!string.IsNullOrEmpty(leakdate))
            {
                DateTime result;
                if (DateTime.TryParse(leakdate, out result))
                {
                    leakdate = result.ToString();
                }
            }
            if (content != null)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);
                string name = htmlDoc.DocumentNode.SelectNodes("//p")?[0].InnerText;
                string website = htmlDoc.DocumentNode.SelectNodes("//p")?[1].InnerText;
                string industry = htmlDoc.DocumentNode.SelectNodes("//p")?[2].InnerText;
                string desc = htmlDoc.DocumentNode.SelectNodes("//p")?[3].InnerText;
                string leaksize = htmlDoc.DocumentNode.SelectSingleNode("//h2[contains(@id,'leaked-data-size-')]")?.InnerText;
                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{IsNA(name)}\n{IsNA(website)}\n\n<b>Industry:</b>\n {IsNA(industry)}\n\n<b>About the victim:</b>\n{IsNA(desc)}\n<b>Publish Date:</b>\n{IsNA(leakdate)}"), TaskCreationOptions.LongRunning);
                task.Wait();
            }
        }
        private static void ProcessPandora(string groupename, XElement item)
        {
            var title = item.Elements().First(i => i.Name.LocalName == "title").Value;
            var content = item.Elements().First(i => i.Name.LocalName == "content").Value;
            var leakdate = item.Elements().First(i => i.Name.LocalName == "updated").Value;
            if (!string.IsNullOrEmpty(leakdate))
            {
                DateTime result;
                if (DateTime.TryParse(leakdate, out result))
                {
                    leakdate = result.ToString();
                }
            }
            if (content != null)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);
                string desc = htmlDoc.DocumentNode.SelectNodes("//p[1]")[0]?.InnerText;
                string leaktime = htmlDoc.DocumentNode.SelectNodes("//h3")?[0].InnerText;
                string leaksize = htmlDoc.DocumentNode.SelectNodes("//h3")[1]?.InnerText;
                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{IsNA(title)}\n\n<b>Data Leak Time:</b>\n {IsNA(leaktime)}\n\n<b>Post date:</b>\n {IsNA(leakdate)}\n<b>About the victim:</b>\n{IsNA(desc)}\n<b>PData Size</b>\n{IsNA(leaksize)}"), TaskCreationOptions.LongRunning);
                task.Wait();
            }
        }
        private static async Task<bool> NavigateMerkatoAsync(string url, string groupename)
        {
            NavigateAndRefreshGui(url);
            try
            {
                HtmlDocument doc = await web.LoadFromWebAsync(url);
                var blocks = doc.DocumentNode.SelectNodes(Consts.MerkatoBlocks);
                if (blocks == null)
                {
                    return false;
                }
                foreach (HtmlNode htmlNode in blocks)
                {
                    HtmlDocument htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlNode.InnerHtml);
                    string name = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'text-left text-grey d-block overflow-hidden')]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                    string leakeddata = htmlDoc.DocumentNode.SelectSingleNode("//span[contains(@class, 'badge badge-pill badge-light mt-1')]")?.InnerText.Replace("\n", string.Empty).TrimEnd().TrimStart();
                    string desc = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'descript-block text-break descript-block-less')]")?.InnerText.TrimEnd().TrimStart();
                    var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{HttpUtility.HtmlDecode(name)} \n\n<b>About the victim:</b>\n{HttpUtility.HtmlDecode(desc)}\n\n<b>{IsNA(leakeddata)} Of leaked data.</b>"), TaskCreationOptions.LongRunning);
                    task.Wait();
                    await Task.Delay(300);
                }
            }
            catch (Exception ex)
            {
                log.Error($"{groupename}\n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
                return false;
            }
            return true;
        }
        private static void NavigateAndRefreshGui(string url)
        {
            try
            {
                Program.mainForm.webView2Control.CoreWebView2.Navigate(url);
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                log.Error($"Fun NavigateAndRefreshGui \n{url}\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
            }
        }
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private static async Task LVPostLoop(string groupename, LV data)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            foreach (Post leak in data.posts)
            {
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(leak.post);
                string safe = htmlDoc.DocumentNode.InnerText;
                var task = Task.Factory.StartNew(() => Tclient.SendAlertAsync($"<b>{IsNA(groupename)}:</b>\n{leak.title} \n<b>Message from the group:</b>\n{safe.Replace("&", string.Empty)} "), TaskCreationOptions.LongRunning);
                task.Wait();
            }
        }
        private static void UpdateStatus(string text)
        {
            try
            {
                Program.mainForm.statusLabel.Text = $"Working on {text}";
                Program.mainForm.webView2Control.CoreWebView2.Settings.UserAgent = RandomUA();
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                log.Error($"Fun UpdateStatus\n" + ex?.Message + Environment.NewLine + ex?.InnerException?.Message);

            }
        }
        private static string RandomUA()
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"UA.txt");
            string[] uas = File.ReadAllLines(path);
            var r = new Random();
            var randomLineNumber = r.Next(0, uas.Length - 1);
            return uas[randomLineNumber];
        }
        public static string Between(string STR, string FirstString, string LastString)
        {
            string FinalString = string.Empty;
            if (!string.IsNullOrEmpty(STR))
            {
                int Pos1 = STR.IndexOf(FirstString) + FirstString.Length;
                int Pos2 = STR.IndexOf(LastString, Pos1);
                FinalString = STR.Substring(Pos1, Pos2 - Pos1);
            }
            return FinalString;
        }

    }
}
