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

        protected override async Task OnInitializedAsync()
        {
            CartService.SetNavMenu(this);
            await CartService.ReloadCartAsync(true);
            Refresh();
        }

        public void Refresh()
        {
            StateHasChanged();  
        }
    }
}
