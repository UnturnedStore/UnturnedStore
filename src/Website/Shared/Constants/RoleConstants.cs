namespace Website.Shared.Constants
{
    public class RoleConstants
    {
        public const string DefaultRoleId = "Guest";
        public const string AdminRoleId = "Admin";
        public const string SellerRoleId = "Seller";
        public const string AdminAndSeller = AdminRoleId + "," + SellerRoleId;

        public static bool IsSeller(string role) => AdminAndSeller.Contains(role);

        public static bool IsNotSeller(string role) => IsSeller(role) == false;
    }
}
