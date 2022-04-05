using log4net;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;

namespace RansomWatcherV2.TwitterSupport
{
    public class Tweet
    {
        readonly string APIKey;
        readonly string APISecret;
        readonly string AccessToken;
        readonly string AccessSecret;
        readonly string hashs;
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public Tweet()
        {
            APIKey = "x";
            APISecret = "x";
            AccessToken = "x-x";
            AccessSecret = "x";
            hashs = "{0}... #ransomware #leaks #infosec #databreach #cyberattack. More @:https://t.me/ransomwatcher"; //190

        }
        public async Task TweetAsync(string tweet)
        {
            try
            {
                TwitterClient client = new TwitterClient(APIKey, APISecret, AccessToken, AccessSecret);
                tweet = Regex.Replace(tweet, @"<a.+?> </a>", "").Replace("<", string.Empty).Replace(">", string.Empty);
                await client.Tweets.PublishTweetAsync(string.Format(hashs, tweet));
                await Task.Delay(600);
            }
            catch (Exception ex)
            {
                log.Error(ex?.Message + Environment.NewLine + ex?.InnerException?.Message);
            }
        }
    }
}
