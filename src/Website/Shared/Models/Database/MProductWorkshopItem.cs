using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Website.Shared.Models.Database
{
    public class MProductWorkshopItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required]
        public ulong WorkshopFileId
        {
            get => (ulong)(DatabaseFileId + long.MaxValue);
            set => DatabaseFileId = (long)(value - long.MaxValue);
        }
        [JsonIgnore]
        public long DatabaseFileId { get; set; } = (long.MinValue + 1);
        public bool IsRequired { get; set; }

        public MProductWorkshopItem() { }

        public MProductWorkshopItem(MProductWorkshopItem workshopItem)
        {
            Id = workshopItem.Id;
            ProductId = workshopItem.ProductId;
            DatabaseFileId = workshopItem.DatabaseFileId;
            IsRequired = workshopItem.IsRequired;
        }
    }
}
