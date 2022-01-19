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
using Website.Client.Pages.Admin.UsersPage.Components;

namespace Website.Client.Pages.Admin.UsersPage
{
    [Authorize(Roles = RoleConstants.AdminRoleId)]
    public partial class UsersPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public IEnumerable<MUser> Users { get; set; }


        public UserModal UserModal { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Users = await HttpClient.GetFromJsonAsync<MUser[]>("api/admin/users");
        }

        private async Task ShowUserAsync(MUser user)
        {
            await UserModal.ShowUserAsync(user);
            StateHasChanged();
        }
    }
}
