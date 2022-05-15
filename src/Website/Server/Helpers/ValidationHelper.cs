using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Steam.Models.SteamCommunity;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Constants;
using Website.Shared.Models.Database;

namespace Website.Server.Helpers
{
    public class ValidationHelper
    {
        private const int SteamIdStartIndex = 37;

        public static async Task SignIn(CookieSignedInContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            UsersRepository usersRepository = context.HttpContext.RequestServices.GetRequiredService<UsersRepository>();
            ImagesRepository imagesRepository = context.HttpContext.RequestServices.GetRequiredService<ImagesRepository>();

            
            string steamId = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value[SteamIdStartIndex..];
            MUser user = await usersRepository.GetUserAsync(steamId);
            
            // Return if user already exists in database
            if (user != null)
            {
                return;
            }

            // Create new user from steamID

            IHttpClientFactory httpClientFactory = context.HttpContext.RequestServices.GetRequiredService<IHttpClientFactory>();
            ILogger<ValidationHelper> logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<ValidationHelper>>();

            SteamWebInterfaceFactory steamFactory = context.HttpContext.RequestServices.GetRequiredService<SteamWebInterfaceFactory>();
            HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(3);

            PlayerSummaryModel playerSummary = null;
            try
            {
                ISteamWebResponse<PlayerSummaryModel> steamWebResponse = await steamFactory.CreateSteamWebInterface<SteamUser>(httpClient).GetPlayerSummaryAsync(ulong.Parse(steamId));
                playerSummary = steamWebResponse.Data;
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

                try
                {
                    byte[] avatarContentBytes = await httpClient.GetByteArrayAsync(playerSummary.AvatarFullUrl);
                    MImage img = new MImage()
                    {
                        Name = "steam_avatar.jpg",
                        Content = avatarContentBytes,
                        ContentType = "image/jpeg"
                    };

                    user.AvatarImageId = await imagesRepository.AddImageAsync(img);
                } 
                catch (Exception e)
                {
                    logger.LogError(e, $"An exception occurated when downloading player avatar {playerSummary.SteamId}");
                }                
            }

            user = await usersRepository.AddUserAsync(user);
        }

        public static async Task Validate(CookieValidatePrincipalContext context)
        {
            string steamId = context.Principal.FindFirst(ClaimTypes.NameIdentifier).Value[SteamIdStartIndex..];

            UsersRepository usersRepository = context.HttpContext.RequestServices.GetRequiredService<UsersRepository>();
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
