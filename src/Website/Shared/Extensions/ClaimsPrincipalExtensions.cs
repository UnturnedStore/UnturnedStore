using System.Security.Claims;

namespace Website.Shared.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int Id(this ClaimsPrincipal principal)
        {
            return int.Parse(principal.Identity.Name);
        }
    }
}
