using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Shared.Models.Database;

namespace Website.Client.Pages.User.CreateMessagePage
{
    [Authorize]
    public partial class CreateMessagePage
    {
        [Parameter]
        public int UserId { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }
        [Inject]
        public NavigationManager NavigationManager { get; set; }

        public MUser User { get; set; }

        private HttpStatusCode statusCode;

        protected override async Task OnParametersSetAsync()
        {
            SetDefault();
            var response = await HttpClient.GetAsync("api/users/" + UserId);
            statusCode = response.StatusCode;
            if (statusCode == HttpStatusCode.OK)
                User = await response.Content.ReadFromJsonAsync<MUser>();
        }

        private void SetDefault()
        {
            Message = defaultMessage;
            Reply = defaultReply;
        }

        public MMessage Message { get; set; }
        private MMessage defaultMessage => new MMessage()
        {
            Replies = new List<MMessageReply>(),
            ToUserId = UserId
        };
        public MMessageReply Reply { get; set; }
        private MMessageReply defaultReply => new MMessageReply()
        {
            Content = null
        };

        private bool isLoading = false;
        private string message = null;
        public async Task SubmitAsync()
        {
            if (string.IsNullOrEmpty(Reply.Content))
            {
                message = "You cannot send empty message";
                return;
            }                

            message = null;
            isLoading = true;

            Message.Replies.Add(Reply);

            HttpResponseMessage response = await HttpClient.PostAsJsonAsync("api/messages", Message);
            MMessage msg = await response.Content.ReadFromJsonAsync<MMessage>();

            SetDefault();
            NavigationManager.NavigateTo($"/messages/{msg.Id}");

            isLoading = false;
        }
    }
}
