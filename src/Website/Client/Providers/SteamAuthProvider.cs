using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Shared.Models;

namespace Website.Client.Providers
{
    public class SteamAuthProvider : AuthenticationStateProvider
    {
        private readonly AuthenticatedUserService userService;

        public SteamAuthProvider(AuthenticatedUserService userService)
        {
            this.userService = userService;
        }

        private static IEnumerable<Claim> GetClaims(UserInfo userInfo)
        {
            return new[]
            {
                new Claim(ClaimTypes.Name, userInfo.Name),
                new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                new Claim("SteamId", userInfo.SteamId),
                new Claim(ClaimTypes.Role, userInfo.Role)
            };
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            ClaimsIdentity identity;
            if (userService.IsAuthenticated)
            {
                IEnumerable<Claim> claims = GetClaims(userService.UserInfo);
                identity = new(claims, "Steam");
            }
            else
            {
                identity = new ClaimsIdentity();
            }

            ClaimsPrincipal user = new(identity);
            AuthenticationState authenticationState = new(user);
            return Task.FromResult(authenticationState);
        }
    }
}
