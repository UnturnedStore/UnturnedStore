using Blazored.TextEditor;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize]
    public partial class CreateMessageUserPage
    {
        [Parameter]
        public int UserId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public UserModel User { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            Message = defaultMessage;
            User = await HttpClient.GetFromJsonAsync<UserModel>("api/users/" + UserId);
        }

        private BlazoredTextEditor editor;

        public MessageModel Message { get; set; }
        private MessageModel defaultMessage => new MessageModel()
        {
            Replies = new List<MessageReplyModel>()
            {
                new MessageReplyModel() { }
            },
            ToUserId = UserId
        };

        public async Task SubmitAsync()
        {
            Message.Replies[0].Content = await editor.GetHTML();
            var response = await HttpClient.PostAsJsonAsync("api/messages", Message);
            var msg = await response.Content.ReadFromJsonAsync<MessageModel>();
            Message = defaultMessage;
            NavigationManager.NavigateTo($"/messages/{msg.Id}");
        }
    }
}
