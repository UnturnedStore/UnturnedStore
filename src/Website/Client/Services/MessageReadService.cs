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
    public class MessageReadService
    {
        private readonly HttpClient httpClient;

        private NavMenu NavMenu { get; set; }
        public void SetNavMenu(NavMenu navMenu)
        {
            NavMenu = navMenu;
        }

        public MessageReadService(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public List<MMessage> Messages { get; private set; }

        public IEnumerable<MMessage> NewMessages => Messages.Where(m => !m.IsClosed && m.Replies.Count > m.Read.ReadId);

        public async Task ReloadMessagesReadAsync()
        {
            Messages = await HttpClient.GetFromJsonAsync<List<MMessage>>("api/messages");
        }

        public bool HasNewMessage(int messageId)
        {
            return NewMessages.Contains(Messages.Find(m => m.Id == messageId));
        }

        public void UpdateMessagesRead(MMessage message)
        {
            Messages.Find(m => m.Id == message.Id) = message;

            if (NavMenu != null)
                NavMenu.Refresh();
        }
    }
}
