using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Client.Services
{
    public class UserService
    {
        private readonly SteamAuthProvider steamAuth;

        public UserService(AuthenticationStateProvider authProvider)
        {
            steamAuth = authProvider as SteamAuthProvider;
        }

        public bool IsAuthenticated => steamAuth.IsAuthenticated;
        public int UserId => steamAuth.User?.Id ?? 0;
        public string Name => steamAuth.User?.Name ?? "null";
        public bool IsAdmin => steamAuth.User?.Role?.Equals(RoleConstants.AdminRoleId) ?? false;
        public MUser User => steamAuth.User;
    }
}
