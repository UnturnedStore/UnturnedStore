using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Website.Shared.Models.Database
{
    public class MProductMedia
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string YoutubeUrl { get; set; }
        public int? ImageId { get; set; }

        public string GetEmbedUrl()
        {
            var uri = new Uri(YoutubeUrl);

            var query = HttpUtility.ParseQueryString(uri.Query);

            var videoId = string.Empty;

            if (query.AllKeys.Contains("v"))
            {
                videoId = query["v"];
            }
            else
            {
                videoId = uri.Segments.Last();
            }

            return "https://www.youtube.com/embed/" + videoId;
        }
    }
}
