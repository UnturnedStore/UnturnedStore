using Blazored.TextEditor;
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
    public partial class MessageUserPage
    {
        [Parameter]
        public int MessageId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        public MessageModel Message { get; set; }

        private BlazoredTextEditor editor;

        protected override async Task OnParametersSetAsync()
        {
            Message = await HttpClient.GetFromJsonAsync<MessageModel>("api/messages/" + MessageId);
        }

        private bool isLoading = false;
        public async Task AddReplyAsync()
        {
            isLoading = true;
            var reply = new MessageReplyModel()
            {
                MessageId = Message.Id,
            };

            reply.Content = await editor.GetHTML();

            var response = await HttpClient.PostAsJsonAsync("api/messages/replies", reply);
            Message.Replies.Add(await response.Content.ReadFromJsonAsync<MessageReplyModel>());
            await editor.LoadHTMLContent(string.Empty);
            isLoading = false;
        }

        public async Task DeleteReplyAsync(MessageReplyModel reply)
        {
            await HttpClient.DeleteAsync("api/messages/replies/" + reply.Id);
            Message.Replies.Remove(reply);
        }

        private string GetUsername(int userId)
        {
            if (Message.FromUserId == userId)
                return Message.FromUser.Name;
            if (Message.ToUserId == userId)
                return Message.ToUser.Name;
            return userId.ToString();
        }
    }
}
