namespace Website.Shared.Models
{
    public class PublicProduct : ProductInfo
    {
        public int TotalDownloadsCount { get; set; }
        public byte AverageRating { get; set; }
        public int RatingsCount { get; set; }
    }
}
