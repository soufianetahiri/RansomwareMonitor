using System;
using System.Collections.Generic;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RansomWatcherV2.Helper
{
    public class RssHelper
    {
        public static IEnumerable<BlogItem> ReadFeed(string url)
        {
            try
            {


                XmlReaderSettings settings = new XmlReaderSettings();
#pragma warning disable CS0618 // Type or member is obsolete
                settings.ProhibitDtd = false;
#pragma warning restore CS0618 // Type or member is obsolete
                WebRequest request = WebRequest.Create(url);
                request.Timeout = 5000;
                using WebResponse response = request.GetResponse();
                using XmlReader reader = XmlReader.Create(response.GetResponseStream(), settings);
                var feed = SyndicationFeed.Load<SyndicationFeed>(reader);
                var result = new List<BlogItem>();

                if (feed == null) return result;

                foreach (var item in feed.Items)
                {
                    var blogItem = ParseFeedItem(item);
                    if (blogItem != null)
                        result.Add(blogItem);
                }
                return result;
            }
            catch
            {
                return null;
            }
        }

        private static BlogItem ParseFeedItem(SyndicationItem item)
        {
            var title = item.Title.Text;
            var summary = item.Summary.Text;
            var publishedDate = item.PublishDate.Date;
            var content = new StringBuilder();
            foreach (var extension in item.ElementExtensions)
            {
                var ele = extension.GetObject<XElement>();
                if (ele.Name.LocalName == "encoded" && ele.Name.Namespace.ToString().Contains("content"))
                {
                    content.Append(ele.Value + " < br / > ");
                }
            }

            return new BlogItem(title, summary, publishedDate, content.ToString());
        }
    }

    public class BlogItem
    {
        public string Title { get; private set; }
        public string Summary { get; private set; }
        public DateTime PublishDate { get; private set; }
        public string Content { get; private set; }

        public BlogItem(string title, string summary, DateTime publishDate, string content)
        {
            Title = title;
            Summary = summary;
            PublishDate = publishDate;
            Content = content;
        }
    }
}
