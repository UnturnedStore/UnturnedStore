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
        private readonly UserService userService;

        private NavMenu NavMenu { get; set; }
        public void SetNavMenu(NavMenu navMenu)
        {
            NavMenu = navMenu;
        }

        public MessageReadService(HttpClient httpClient, UserService userService)
        {
            this.httpClient = httpClient;
            this.userService = userService;
        }

        public List<MMessage> Messages { get; set; }

        public IEnumerable<MMessage> NewMessages => Messages.Where(m => !m.IsClosed && (m.Replies.Count <= 1 ? 0 : m.Replies[m.Replies.Count - 1].Id) > (m.Read?.ReadId ?? -1));

        public async Task ReloadMessagesReadAsync()
        {
            //if (!userService.IsAuthenticated) return;

            //Messages = await httpClient.GetFromJsonAsync<List<MMessage>>("api/messages");

            // UserService isn't initlized when this function is called so that's why I am doing this terriblness
            // If someone can fix it please do!
            try
            {
                var response = await httpClient.GetAsync("api/messages");
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    Messages = await response.Content.ReadFromJsonAsync<List<MMessage>>();
                }
            }
            catch (Exception) { }

            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public bool HasNewMessage(int messageId)
        {
            return NewMessages.Any(m => m.Id == messageId);
        }

        public void UpdateMessagesRead(MMessage message)
        {
            var msg = Messages.FindIndex(m => m.Id == message.Id);
            if (msg == -1) Messages.Add(message);
            else Messages[msg] = message;

            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public void UpdateMessagesRead(List<MMessage> messages)
        {
            Messages = messages;

            if (NavMenu != null)
                NavMenu.Refresh();
        }
    }
}
