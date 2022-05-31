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
using Website.Data.Repositories;

namespace Website.Client.Services
{
    public class MessageService
    {
        private readonly UserService userService;
        private readonly UsersRepository usersRepository;
        private readonly HttpClient httpClient;

        private NavMenu NavMenu { get; set; }
        public void SetNavMenu(NavMenu navMenu)
        {
            NavMenu = navMenu;
        }

        public MessageService(UserService userService, UsersRepository usersRepository, HttpClient httpClient)
        {
            this.userService = userService;
            this.usersRepository = usersRepository;
            this.httpClient = httpClient;
        }
        
        public List<MMessage> Messages { get; private set; }
        
        public IEnumerable<MMessage> NewMessages => Messages.Where(m => m.CreateDate > userService.User.LastAccessedMessages
                                                            || m.Replies.Any(mr => mr.CreateDate > userService.User.LastAccessedMessages));
        
        public async Task ReloadMessagesAsync()
        {
            Messages = await HttpClient.GetFromJsonAsync<List<MMessage>>("api/messages");
            if (Messages == null)
                Messages = new List<MMessage>();

            if (NavMenu != null)
                NavMenu.Refresh();
        }

        public async Task RefreshMessages(List<MMessage> messages)
        {
            userService.User.LastAccessedMessages = DateTime.UtcNow;
            var user = MUser.FromUser(userService.User);
            await usersRepository.UpdateLastAccessedMessages(user);

            if (NewMessages.Count() > 0 && NavMenu != null)
            {
                Messages = messages;
                NavMenu.Refresh();
            } else
            {
                Messages = messages;
            }
        }
    }
}
