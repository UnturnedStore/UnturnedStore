using System.Collections.Generic;
using Website.Shared.Models.Database;

namespace Website.Shared.Models
{
    public class FullPublicProduct : PublicProduct
    {
        public UserInfo Customer { get; set; }

        public List<MProductTab> Tabs { get; set; }
        public List<MProductMedia> Media { get; set; }
        public List<MProductReview> Reviews { get; set; }
        public List<MProductWorkshopItem> WorkshopItems { get; set; }
        public List<MBranch> Branches { get; set; }
    }
}
