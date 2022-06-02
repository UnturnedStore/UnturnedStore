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
        public UserService UserService { get; set; }
        [Inject]
        public MessageReadService MessageReadService { get; set; }

        protected override void OnInitialized()
        {
            CartService.SetNavMenu(this);
            MessageReadService.SetNavMenu(this);
            Refresh();
        }

        public void Refresh()
        {
            StateHasChanged();  
        }
    }
}
