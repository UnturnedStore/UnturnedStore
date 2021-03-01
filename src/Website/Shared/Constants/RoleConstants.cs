using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Constants
{
    public class RoleConstants
    {
        public const string DefaultRoleId = "Guest";
        public const string AdminRoleId = "Admin";
        public const string SellerRoleId = "Seller";

        public const string AdminAndSeller = AdminRoleId + "," + SellerRoleId;
    }
}
