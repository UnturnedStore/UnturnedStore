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
using Website.Client.Services;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.User.MessagesPage
{
    [Authorize]
    public partial class MessagesPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AuthenticationStateProvider AuthState { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }
        [Inject]
        public AuthenticatedUserService UserService { get; set; }
        [Inject]
        public MessageReadService MessageReadService { get; set; }

        public List<MMessage> Messages { get; set; }

        public IEnumerable<MMessage> ActiveMessages => Messages.Where(x => !x.IsClosed);
        public IEnumerable<MMessage> ClosedMessages => Messages.Where(x => x.IsClosed);

        protected override async Task OnInitializedAsync()
        {
            Messages = await HttpClient.GetFromJsonAsync<List<MMessage>>("api/messages");
            MessageReadService.UpdateMessagesRead(Messages);
        }

        private void GoToMessage(MMessage msg)
        {
            NavigationManager.NavigateTo("/messages/" + msg.Id);
        }
    }
}
