using System.Collections.Generic;
using Website.Shared.Models.Database;

namespace Website.Shared.Models.Children
{
    public class UserProfile : MUser
    {
        public int Sales { get; set; }
        public List<MProduct> Products { get; set; }
    }
}
