using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Server.Helpers
{
    public class ValidationHelper
    {
        private const int SteamIdStartIndex = 37;

        public static async Task SignIn(CookieSignedInContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            string steamId = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value[SteamIdStartIndex..];

            var usersRepository = context.HttpContext.RequestServices.GetRequiredService<UsersRepository>();
            var imagesRepository = context.HttpContext.RequestServices.GetRequiredService<ImagesRepository>();

            MUser user = await usersRepository.GetUserAsync(steamId);

            if (user != null)
            {
                return;
            }

            var steamFactory = context.HttpContext.RequestServices.GetRequiredService<SteamWebInterfaceFactory>();
            var httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ValidationHelper>>();

            var client = httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);

            PlayerSummaryModel playerSummary = null;

            try
            {
                var response = await steamFactory.CreateSteamWebInterface<SteamUser>(client).GetPlayerSummaryAsync(ulong.Parse(steamId));
                playerSummary = response.Data;
            }
            catch (Exception e)
            {
                logger.LogError(e, "An exception occurated when downloading player summaries");
            }

            user = new MUser()
            {
                SteamId = steamId,
                Role = RoleConstants.DefaultRoleId
            };

            if (playerSummary != null)
            {
                user.Name = playerSummary.Nickname;
            }

            if (playerSummary != null)
            {
                byte[] avatarContent = await client.GetByteArrayAsync(playerSummary.AvatarFullUrl);
                MImage img = new MImage()
                {
                    Name = "steam_avatar.jpg",
                    Content = avatarContent,
                    ContentType = "image/jpeg"
                };

                user.AvatarImageId = await imagesRepository.AddImageAsync(img);
            }

            user = await usersRepository.AddUserAsync(user);
        }

        public static async Task Validate(CookieValidatePrincipalContext context)
        {
            string steamId = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value[SteamIdStartIndex..];

            var usersRepository = context.HttpContext.RequestServices.GetRequiredService<UsersRepository>();

            MUser user = await usersRepository.GetUserAsync(steamId);

            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            context.ReplacePrincipal(new ClaimsPrincipal(new ClaimsIdentity(claims, "Steam")));
        }
    }
}
