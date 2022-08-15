using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace Website.Shared.Results
{
    public class WorkshopItemResult
    {
        public static string WorkshopItemsUrl()
        {
            const string BaseUrl = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/?";

            return BaseUrl;
        }

        public static FormUrlEncodedContent BuildWorkshopItemsFormData(params ulong[] FileIds)
        {
            Dictionary<string, string> FormData = new Dictionary<string, string>()
            {
                { "itemcount", FileIds.Length.ToString() }
            };

            for (int i = 0; i < FileIds.Length; i++)
                FormData.Add($"publishedfileids[{i}]", FileIds[i].ToString());

            return new FormUrlEncodedContent(FormData);
        }

        public Response response { get; set; }

        public bool IsSuccessItem(Publishedfiledetail item) => item.result == 1 && item.banned == 0 && item.creator_app_id == 304930;
        public IEnumerable<Publishedfiledetail> SuccessItems() => response.publishedfiledetails.Where(x => IsSuccessItem(x));
        public Publishedfiledetail GetSuccessItem(ulong FileId) => response.publishedfiledetails.FirstOrDefault(x => IsSuccessItem(x) && x.FileId() == FileId);
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
        public string Description()
        {
            string desc = description;

            desc = Regex.Replace(desc, "<.*?>|\\[.*?\\]", string.Empty);
            desc = desc.Replace(Environment.NewLine, "<br>");

            var splitDesc = desc.Split("<br>", StringSplitOptions.TrimEntries);
            int lines = (int)((float)splitDesc.Length / 21f);
            if (lines < 7) lines = 7;

            if (splitDesc.Length > 7)
            {
                if (splitDesc[6].Length > 80) splitDesc[6] = splitDesc[6].Substring(0, 80);
                return string.Join("<br>", splitDesc.SkipLast(splitDesc.Length - 7)).TrimEnd(' ') + "...";
            }

            return desc;
        }
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
