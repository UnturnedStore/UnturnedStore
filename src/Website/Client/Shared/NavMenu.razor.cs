using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Website.Client.Services;

namespace Website.Client.Shared
{
    public partial class NavMenu
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        [Inject]
        public CartService CartService { get; set; }
        [Inject]
        public AuthenticatedUserService UserService { get; set; }
        [Inject]
        public MessageReadService MessageReadService { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        protected override void OnInitialized()
        {
            CartService.SetNavMenu(this);
            MessageReadService.SetNavMenu(this);
            Refresh();
        }

        protected override async Task OnInitializedAsync()
        {
            if (NavigationManager.Uri.Replace(NavigationManager.BaseUri, "").Replace("/", "") != "messages")
                await MessageReadService.ReloadMessagesReadAsync();
        }

        public void Refresh()
        {
            StateHasChanged();  
        }
    }
}
