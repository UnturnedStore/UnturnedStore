using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Website.Shared.Results
{
    public class WorkshopItemResult
    {
        public static string BuildWorkshopItemsUrl(params ulong[] FileIds)
        {
            const string BaseUrl = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/?";

            StringBuilder urlBuilder = new StringBuilder();
            urlBuilder.Append(BaseUrl);
            urlBuilder.Append($"&itemcount={FileIds.Length}");
            for (int i = 0; i < FileIds.Length; i++)
                urlBuilder.Append($"&publishedfileids[{i}]={FileIds[i]}");

            return WebUtility.UrlEncode(urlBuilder.ToString()); ;
        }

        public Response response { get; set; }

        public bool IsSuccessItem(Publishedfiledetail item) => item.result == 1 && item.creator_app_id == 304930;
        public IEnumerable<ulong> SuccessItems() => response.publishedfiledetails.Where(x => IsSuccessItem(x)).Select(x => x.FileId());
        public Publishedfiledetail GetItem(ulong FileId) => response.publishedfiledetails.FirstOrDefault(x => IsSuccessItem(x) && x.FileId() == FileId);
    }

    public class Response
    {
        public uint result { get; set; }
        public uint resultcount { get; set; }
        public Publishedfiledetail[] publishedfiledetails { get; set; }
    }

    public class Publishedfiledetail
    {
        public string publishedfileid { get; set; }
        public ulong FileId() => ulong.Parse(publishedfileid);
        public int result { get; set; }
        public string creator { get; set; }
        public int creator_app_id { get; set; }
        public int consumer_app_id { get; set; }
        public string filename { get; set; }
        public int file_size { get; set; }
        public string file_url { get; set; }
        public string hcontent_file { get; set; }
        public string preview_url { get; set; }
        public string hcontent_preview { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int time_created { get; set; }
        public int time_updated { get; set; }
        public int visibility { get; set; }
        public int banned { get; set; }
        public string ban_reason { get; set; }
        public int subscriptions { get; set; }
        public int favorited { get; set; }
        public int lifetime_subscriptions { get; set; }
        public int lifetime_favorited { get; set; }
        public int views { get; set; }
        public Tag[] tags { get; set; }
    }

    public class Tag
    {
        public string tag { get; set; }
    }
}
