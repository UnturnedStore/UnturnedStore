using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Services;
using Website.Shared.Models;

namespace Website.Client.Pages.Home.UserPage
{
    public partial class UserPage
    {
        [Parameter]
        public int UserId { get; set; }
        
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public UserService UserService { get; set; }

        public MUser User { get; set; }

        protected override async Task OnInitializedAsync()
        {
            User = await HttpClient.GetFromJsonAsync<MUser>($"api/users/{UserId}/profile");
        }
    }
}
