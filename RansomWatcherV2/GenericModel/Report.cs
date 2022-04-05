
using System;
using System.Collections.Generic;

namespace RansomWatcherV2.GenericModel
{
    public class Report
    {
        public string Summary { get; set; }
        public List<string> VictimeName { get; set; }
        public List<string> VictimeWebsite { get; set; }
        public List<string> PublishDate { get; set; }
        public List<string> UpdateDate { get; set; }
        public List<string> Views { get; set; }
    }
    public class HiveLeakCompanies
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string country { get; set; }
        public string tax_number { get; set; }
        public string website { get; set; }
        public string revenue { get; set; }
        public string employees { get; set; }
        public string disclose_at { get; set; }
        public string encrypted_at { get; set; }
        public string disclosed_at { get; set; }
    }
    public class HiveLeak
    {
        public string id { get; set; }
        public string company_id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string url { get; set; }
        public long? size { get; set; }
        public string password { get; set; }
        public string status { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
    }
    public class ContiLeak
    {
        public string code { get; set; }
        public int view { get; set; }
        public int highlights { get; set; }
        public int published { get; set; }
        public string links { get; set; }
        public string title { get; set; }
        public string address { get; set; }
        public string about { get; set; }
        public string url { get; set; }
        public int date { get; set; }
    }
    public class Ragnar
    {
        public string link { get; set; }
        public string title { get; set; }
        public string views { get; set; }
        public string password { get; set; }
        public string timestamp { get; set; }
        public string info { get; set; }
    }

    public class Datum
    {
        public string title { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
        public string views { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string published_at { get; set; }
        public string reject_type { get; set; }
        public string files_count { get; set; }
    }

    public class LatestCompl
    {
        public string title { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
        public string views { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string published_at { get; set; }
        public string reject_type { get; set; }
    }

    public class LatestProg
    {
        public string title { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
        public string views { get; set; }
        public string created_at { get; set; }
        public string updated_at { get; set; }
        public string published_at { get; set; }
        public string reject_type { get; set; }
    }

    public class Grief
    {
        public List<Datum> data { get; set; }
        public int page { get; set; }
        public int per_page { get; set; }
        public int total { get; set; }
        public int total_pages { get; set; }
        public List<LatestCompl> latest_compl { get; set; }
        public List<LatestProg> latest_progs { get; set; }
    }
    public class Post
    {
        public DateTime date { get; set; }
        public string link { get; set; }
        public string post { get; set; }
        public string title { get; set; }
    }

    public class LV
    {
        public int page { get; set; }
        public int page_size { get; set; }
        public int pages_count { get; set; }
        public List<Post> posts { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class Item
    {
        public string id { get; set; }
        public string title { get; set; }
        public string previewContent { get; set; }
        public string content { get; set; }
        public long createdDt { get; set; }
    }

    public class AlphVm
    {
        public List<Item> items { get; set; }
        public int total { get; set; }
    }

}
