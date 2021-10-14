using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

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
