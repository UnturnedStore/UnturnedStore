using System.Collections.Generic;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Shared.Models
{
    public class SellerProduct : PrivateProduct
    {
        public UserInfo Admin { get; set; }

        public List<MProductTab> Tabs { get; set; }
        public List<MProductMedia> Media { get; set; }
        public List<MProductReview> Reviews { get; set; }
        public List<MProductWorkshopItem> WorkshopItems { get; set; }
        public List<MBranch> Branches { get; set; }
        public List<MProductTag> Tags { get; set; }

        public MProduct ToMProduct()
        {
            return new MProduct()
            {
                Id = Id,
                Price = Price,
                Description = Description,
                Category = Category,
                GithubUrl = GithubUrl,
                Name = Name,
                ImageId = ImageId,
                SellerId = Seller.Id,
                Status = Status,
                StatusReason = StatusReason,
                AdminId = AdminId,
                IsLoaderEnabled = IsLoaderEnabled,
                IsEnabled = IsEnabled,
                LastUpdate = LastUpdate,
                CreateDate = CreateDate,
                Tags = Tags
            };
        }

        public bool IsRocketPlugin => Category == ProductCategoryConstants.RocketPlugin;
    }
}
