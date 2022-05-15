using System.Security.Claims;

namespace Website.Shared.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int Id(this ClaimsPrincipal principal)
        {
            return int.Parse(principal.Identity.Name);
        }

        public static bool TryGetId(this ClaimsPrincipal principal, out int userId)
        {
            userId = 0;
            if (!string.IsNullOrWhiteSpace(principal.Identity?.Name ?? null))
            {
                int.TryParse(principal.Identity.Name, out userId);
                return true;
            }

            return false;
        }
    }
}
