using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;

namespace Website.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesRepository messagesRepository;

        public MessagesController(MessagesRepository messagesRepository)
        {
            this.messagesRepository = messagesRepository;
        }
    }
}
