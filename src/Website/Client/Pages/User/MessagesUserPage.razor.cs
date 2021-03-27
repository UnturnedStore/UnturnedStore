using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Shared.Models;

namespace Website.Client.Pages.User
{
    [Authorize]
    public partial class MessagesUserPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AuthenticationStateProvider AuthState { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        private SteamAuthProvider SteamAuth => AuthState as SteamAuthProvider;

        public List<MessageModel> Messages { get; set; }

        public IEnumerable<MessageModel> ActiveMessages => Messages.Where(x => !x.IsClosed);
        public IEnumerable<MessageModel> ClosedMessages => Messages.Where(x => x.IsClosed);

        protected override async Task OnInitializedAsync()
        {
            Messages = await HttpClient.GetFromJsonAsync<List<MessageModel>>("api/messages");
        }

        private void GoToMessage(MessageModel msg)
        {
            NavigationManager.NavigateTo("/messages/" + msg.Id);
        }
    }
}
