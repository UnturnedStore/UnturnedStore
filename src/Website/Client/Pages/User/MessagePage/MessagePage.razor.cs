using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Shared.Models;
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
        [Inject]
        public MessageReadService MessageReadService { get; set; }

        public SteamAuthProvider steamAuth => AuthState as SteamAuthProvider;

        public MMessage Message { get; set; }
            
        private HttpStatusCode statusCode;

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync("api/messages/" + MessageId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
            {
                Message = await response.Content.ReadFromJsonAsync<MMessage>();
                SetDefault();

                var responseRead = await HttpClient.GetAsync("api/messages/read/" + MessageId + "/" + steamAuth.User.Id);
                Message.Read = await responseRead.Content.ReadFromJsonAsync<MMessageRead>();
                if (Message.Read == null)
                {
                    Message.Read = await HttpClient.PostAsJsonAsync("api/messages/read", newlyRead);
                }
                else
                {
                    await HttpClient.PutAsJsonAsync("api/messages/read", newlyRead);
                    Message.Read = newlyRead;
                }
                
                MessageReadService.UpdateMessagesRead(Message);
            }
        }

        private MMessageReply Reply { get; set; }
        private MMessageReply defaultReply => new MMessageReply()
        {
            MessageId = Message.Id
        };

        private MMessageRead newlyRead => new MMessageRead()
        {
            Id = Message.Read.Id,
            MessageId = Message.Id,
            UserId = steamAuth.User.Id,
            ReadId = Message.Replies.Count == 0 ? 0 : Message.Replies[Message.Replies.Count - 1].Id
        };

        private void SetDefault()
        {
            Reply = defaultReply;
        }

        private string msg = null;
        private bool isLoading = false;
        public async Task AddReplyAsync()
        {
            if (string.IsNullOrEmpty(Reply.Content))
            {
                msg = "You cannot send empty message";
                return;
            }

            msg = null;
            isLoading = true;

            var response = await HttpClient.PostAsJsonAsync("api/messages/replies", Reply);
            SetDefault();
            Message.Replies.Add(await response.Content.ReadFromJsonAsync<MMessageReply>());

            if (Message.Read != null)
            {
                await HttpClient.PutAsJsonAsync("api/messages/read", newlyRead);
                Message.Read = newlyRead;
                MessageReadService.UpdateMessagesRead(Message);
            }

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

        private UserInfo User(int userId)
        {
            if (Message.FromUserId == userId)
                return Message.FromUser;
            if (Message.ToUserId == userId)
                return Message.ToUser;

            return null;
        }
    }
}
