using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Website.Shared.Models.Database
{
    public class MProductWorkshopItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [Required]
        public ulong UseableFileId
        {
            get => (ulong)(WorkshopFileId + long.MaxValue);
            set => WorkshopFileId = (long)(value - long.MaxValue);
        }
        [JsonIgnore]
        public long WorkshopFileId { get; set; } = (long.MinValue + 1);
        public bool IsRequired { get; set; }

        public MProductWorkshopItem() { }

        public MProductWorkshopItem(MProductWorkshopItem workshopItem)
        {
            Id = workshopItem.Id;
            ProductId = workshopItem.ProductId;
            WorkshopFileId = workshopItem.WorkshopFileId;
            IsRequired = workshopItem.IsRequired;
        }
    }
}
