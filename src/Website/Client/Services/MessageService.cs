using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Website.Client.Shared;
using Website.Shared.Constants;
using Website.Shared.Models;
using Website.Shared.Models.Database;
using Website.Shared.Params;

namespace Website.Client.Services
{
    public class MessageService
    {
        private readonly UserService userService;
        private readonly HttpClient httpClient;

        private NavMenu NavMenu { get; set; }
        public void SetNavMenu(NavMenu navMenu)
        {
            NavMenu = navMenu;
        }

        public MessageService(UserService userService, HttpClient httpClient)
        {
            this.userService = userService;
            this.httpClient = httpClient;
        }
        
        public List<MMessage> Messages { get; private set; }
        
        public List<MMessage> NewMessages => Messages.Where(m => m.CreateDate > userService.User.LastAccessedMessages
                                                            || m.Replies.Any(mr => mr.CreateDate > userService.User.LastAccessedMessages))
                                                            .ToList();
        
        public async Task ReloadMessagesAsync()
        {
            Messages = await HttpClient.GetFromJsonAsync<List<MMessage>>("api/messages");
            if (Messages == null)
                Messages = new List<MMessage>();

            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public void RefreshMessages(List<MMessage> messages)
        {
            Messages = messages;
            if (NavMenu != null)
                NavMenu.Refresh();
        }
    }
}
