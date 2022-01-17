using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models.Database;
using Website.Shared.Models;

namespace Website.Client.Pages.Admin.UsersPage
{
    [Authorize(Roles = RoleConstants.AdminRoleId)]
    public partial class UsersPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public IEnumerable<UserInfo> Users { get; set; }
        private ICollection<UserInfo> orderedUsers => Users.Where(x => string.IsNullOrEmpty(searchString) 
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) 
            || x.SteamId == searchString)
            .OrderByDescending(x => x.CreateDate).ToList();

        private string searchString = string.Empty;

        private List<UserInfo> loadingUsers = new List<UserInfo>();

        protected override async Task OnInitializedAsync()
        {
            Users = await HttpClient.GetFromJsonAsync<UserInfo[]>("api/users");
        }

        private async Task UpdateUserRoleAsync(UserInfo user)
        {
            loadingUsers.Add(user);
            await HttpClient.PutAsJsonAsync("api/admin/users", user);
            loadingUsers.Remove(user);
        }
    }
}
