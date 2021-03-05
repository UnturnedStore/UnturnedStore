using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Client.Pages.User
{
    public partial class MessagesUserPage
    {
        [Inject]
        public HttpClient HttpClient { get; set; }

        public List<MessageModel> Messages { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Messages = await HttpClient.GetFromJsonAsync<List<MessageModel>>("api/messages");
        }
    }
}
