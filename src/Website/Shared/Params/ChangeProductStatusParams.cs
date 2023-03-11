using System.Text.Json.Serialization;
using Website.Shared.Enums;
using Website.Shared.Models;

namespace Website.Shared.Params
{
    public class ChangeProductStatusParams
    {
        public int ProductId { get; set; }
        public ProductStatus Status { get; set; }
        public string StatusReason { get; set; }

        [JsonIgnore]
        public int? AdminId { get; set; }
        [JsonIgnore]
        public ProductInfo Product { get; set; }
    }
}
