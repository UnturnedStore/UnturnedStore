using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Providers;
using Website.Client.Shared;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Client.Services
{
    public class MessageReadService
    {
        private readonly HttpClient httpClient;
        private readonly AuthenticatedUserService userService;

        private NavMenu NavMenu { get; set; }
        public void SetNavMenu(NavMenu navMenu)
        {
            NavMenu = navMenu;
        }

        public MessageReadService(HttpClient httpClient, AuthenticatedUserService userService)
        {
            this.httpClient = httpClient;
            this.userService = userService;
        }

        public List<MMessageRead> NewMessages { get; set; }

        public bool HasNewMessages => NewMessages != null && NewMessages.Count > 0;

        public async Task ReloadMessagesReadAsync()
        {
            if (!userService.IsAuthenticated) return;

            NewMessages = await httpClient.GetFromJsonAsync<List<MMessageRead>>("api/messages/read");

            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public static bool HasNewMessage(MMessage message)
        {
            return !message.IsClosed && (message.Replies.Count == 0 ? 0 : message.Replies[message.Replies.Count - 1].Id) > (message.Read?.ReadId ?? -1);
        }

        public void UpdateMessagesRead(MMessage message)
        {
            NewMessages.Remove(message.Read);

            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public void UpdateMessagesRead(List<MMessage> messages)
        {
            NewMessages = messages.Where(m => HasNewMessage(m)).Select(m => m.Read ?? new MMessageRead(m, userService.UserId)).ToList();

            if (NavMenu != null)
                NavMenu.Refresh();
        }
    }
}
