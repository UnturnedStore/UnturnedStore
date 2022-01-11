using Blazored.TextEditor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.User.MessagePage
{
    [Authorize]
    public partial class MessagePage
    {
        [Parameter]
        public int MessageId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public AuthenticationStateProvider AuthState { get; set; }

        public SteamAuthProvider steamAuth => AuthState as SteamAuthProvider;

        public MMessage Message { get; set; }

        private BlazoredTextEditor editor;
    
        private HttpStatusCode statusCode;

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync("api/messages/" + MessageId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
                Message = await response.Content.ReadFromJsonAsync<MMessage>();
        }

        private string msg = null;
        private bool isLoading = false;
        public async Task AddReplyAsync()
        {
            string content = await editor.GetHTML();
            if (content == "<p><br></p>")
            {
                msg = "You cannot send empty message";
                return;
            }
            msg = null;

            isLoading = true;
            var reply = new MMessageReply()
            {
                MessageId = Message.Id
            };

            reply.Content = content;
            var response = await HttpClient.PostAsJsonAsync("api/messages/replies", reply);
            Message.Replies.Add(await response.Content.ReadFromJsonAsync<MMessageReply>());
            await editor.LoadHTMLContent(string.Empty);
            isLoading = false;
        }

        public async Task DeleteReplyAsync(MMessageReply reply)
        {
            await HttpClient.DeleteAsync("api/messages/replies/" + reply.Id);
            Message.Replies.Remove(reply);
        }

        public async Task CloseAsync()
        {
            Message.IsClosed = true;
            Message.ClosingUserId = steamAuth.User.Id;
            await HttpClient.PatchAsync("api/messages/" + MessageId, null);            
        }

        private MUser User(int userId)
        {
            if (Message.FromUserId == userId)
                return Message.FromUser;
            if (Message.ToUserId == userId)
                return Message.ToUser;

            return null;
        }
    }
}
