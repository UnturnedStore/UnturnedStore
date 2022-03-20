using System.Text.Json.Serialization;

namespace Website.Shared.Models
{
    public class Seller : UserInfo
    {
        public bool IsPayPalEnabled { get; set; }
        public string PayPalAddress { get; set; }
        public bool IsNanoEnabled { get; set; }
        public string NanoAddress { get; set; }
        [JsonIgnore]
        public string DiscordWebhookUrl { get; set; }
        public string TermsAndConditions { get; set; }
        public bool IsVerifiedSeller { get; set; }
    }
}
