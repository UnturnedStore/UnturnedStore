using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Client.Services;
using Website.Components.Basic;
using Website.Components.Alerts;
using Website.Shared.Models;
using Website.Shared.Models.Database;
using System.Linq;
using Website.Client.Pages.User.MessagePage.Components;

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
        public AuthenticatedUserService UserService { get; set; }
        [Inject]
        public MessageReadService MessageReadService { get; set; }

        public MMessage Message { get; set; }
        
        private int? NewReplyId { get; set; }
        private bool IsHighlighterHidden { get; set; }
            
        private HttpStatusCode statusCode;

        protected override async Task OnParametersSetAsync()
        {
            var response = await HttpClient.GetAsync("api/messages/" + MessageId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
            {
                Message = await response.Content.ReadFromJsonAsync<MMessage>();
                SetDefault();

                if (MessageReadService.HasNewMessage(Message))
                {
                    if (Message.Read == null)
                    {
                        var responseRead = await HttpClient.PostAsJsonAsync("api/messages/read", newlyRead);
                        Message.Read = await responseRead.Content.ReadFromJsonAsync<MMessageRead>();
                    }
                    else
                    {
                        NewReplyId = Message.Replies.First(r => r.Id > Message.Read.ReadId).Id;

                        await HttpClient.PutAsJsonAsync("api/messages/read", newlyRead);
                        Message.Read = newlyRead;
                    }

                    MessageReadService.UpdateMessagesRead(Message);
                }
            }
        }

        private MMessageReply Reply { get; set; }
        private MMessageReply defaultReply => new MMessageReply()
        {
            MessageId = Message.Id
        };

        private MMessageRead newlyRead => new MMessageRead()
        {
            Id = Message.Read?.Id ?? 0,
            MessageId = Message.Id,
            UserId = UserService.UserId,
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
            if (NewReplyId.HasValue) IsHighlighterHidden = true;
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

        public ConfirmModal<MMessage> ConfirmClose { get; set; }

        private async Task ShowCloseMessageAsync()
        {
            await ConfirmClose.ShowAsync(Message);
        }

        public async Task CloseMessageAsync()
        {
            Message.IsClosed = true;
            Message.ClosingUserId = UserService.UserId;
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
