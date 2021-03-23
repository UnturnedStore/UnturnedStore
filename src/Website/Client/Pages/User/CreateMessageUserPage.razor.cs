using Blazored.TextEditor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

        private HttpStatusCode statusCode;

        protected override async Task OnParametersSetAsync()
        {
            Message = defaultMessage;
            var response = await HttpClient.GetAsync("api/users/" + UserId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
                User = await response.Content.ReadFromJsonAsync<UserModel>();
        }

        private BlazoredTextEditor editor;

        public MessageModel Message { get; set; }
        private MessageModel defaultMessage => new MessageModel()
        {
            Replies = new List<MessageReplyModel>(),
            ToUserId = UserId
        };

        public async Task SubmitAsync()
        {
            Message.Replies.Add(new MessageReplyModel()
            {
                Content = await editor.GetHTML()
            });

            var response = await HttpClient.PostAsJsonAsync("api/messages", Message);
            var msg = await response.Content.ReadFromJsonAsync<MessageModel>();
            Message = defaultMessage;
            NavigationManager.NavigateTo($"/messages/{msg.Id}");
        }
    }
}
