using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesRepository messagesRepository;

        public MessagesController(MessagesRepository messagesRepository)
        {
            this.messagesRepository = messagesRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetMessagesAsync()
        {
            return Ok(await messagesRepository.GetMessagesAsync(int.Parse(User.Identity.Name)));
        }

        [HttpGet("{messageId}")]
        public async Task<IActionResult> GetMessageAsync(int messageId)
        {
            if (!await messagesRepository.IsMessageUserAsync(messageId, int.Parse(User.Identity.Name)))
                return BadRequest();

            return Ok(await messagesRepository.GetMessageAsync(messageId));
        }

        [HttpPost]
        public async Task<IActionResult> PostMessageAsync([FromBody] MessageModel message)
        {
            int userId = int.Parse(User.Identity.Name);
            message.FromUserId = userId;
            foreach (var reply in message.Replies)
            {
                reply.UserId = userId;
            }

            return Ok(await messagesRepository.AddMessageAsync(message));
        }
        
        [HttpPost("replies")]
        public async Task<IActionResult> PostMessageReplyAsync([FromBody] MessageReplyModel reply)
        {
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageReplyUserAsync(reply.Id, userId))
                return BadRequest();

            reply.UserId = userId;

            return Ok(await messagesRepository.AddMessageReplyAsync(reply));
        }

        [HttpPut("replies")]
        public async Task<IActionResult> PutMessageReplyAsync([FromBody] MessageReplyModel reply)
        {
            if (!await messagesRepository.IsMessageReplyUserAsync(reply.Id, int.Parse(User.Identity.Name)))
                return BadRequest();

            await messagesRepository.UpdateMessageReplyAsync(reply);
            return Ok();
        }
    }
}
