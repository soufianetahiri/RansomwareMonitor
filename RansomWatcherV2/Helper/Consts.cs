using System.IO;
using System.Reflection;

namespace RansomWatcherV2.Helper
{
    public static class Consts
    {
        public static string ChatId;
        public static string TokenId;
        public static string NA = "N/A";
        public static string HistoryTemp = Path.GetTempPath() + "ransomwatcherHistory.db";
        public static string Historydb = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\" + "history.db";
        public static string ragnarXpath = "//script[@type='text/javascript' and contains(., 'post_links')]";
        public static string griefVictimeXpath = "//a[contains(@class, 'text-dark')]";
        public static string griefVictimeWebsiteXpath = "//a[contains(@class, 'text-info')]";
        public static string griefUpdatedXpath = "//small[3]";
        public static string griefPublishedXpath = "//small[2]";
        public static string grieVieweddXpath = "//small[1]";
        public static string pysaLeakblock = "//div[contains(@class, 'page-header')]";
        public static string ransomExxLeakkblock = "//div[contains(@class, 'card-body')]";
        public static string snatchLeakkblock = "//div[contains(@class, 'ann-block')]";
        public static string LockbitBlock = "//a[contains(@class, 'post-more-link')]";
        public static string LockbiPostTitleBlock = "//div[contains(@class, 'post-title-block')]";
        public static string XingBlocks = "//div[contains(@class, 'row g-0 border rounded overflow-hidden flex-md-row mb-4 shadow-sm h-md-250 position-relative')]";
        public static string AtomSiloBlocks = "//div[contains(@class, 'card card-post bg-black-25')]";
        public static string MerkatoBlocks = "//div[contains(@class, 'row lot flex-column m-0')]";
    }
}
