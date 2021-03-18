using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Constants;
using Website.Shared.Models;

namespace Website.Client.Pages.Admin
{
    [Authorize(Roles = RoleConstants.AdminRoleId)]
    public partial class UsersAdminPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public IEnumerable<UserModel> Users { get; set; }
        private ICollection<UserModel> orderedUsers => Users.Where(x => string.IsNullOrEmpty(searchString) 
            || x.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase) 
            || x.SteamId == searchString)
            .OrderByDescending(x => x.CreateDate).ToList();

        private string searchString = string.Empty;

        private List<UserModel> loadingUsers = new List<UserModel>();

        protected override async Task OnInitializedAsync()
        {
            Users = await HttpClient.GetFromJsonAsync<UserModel[]>("api/users");
        }

        private async Task UpdateUserRoleAsync(UserModel user)
        {
            loadingUsers.Add(user);
            await HttpClient.PutAsJsonAsync("api/admin/users", user);
            loadingUsers.Remove(user);
        }
    }
}
