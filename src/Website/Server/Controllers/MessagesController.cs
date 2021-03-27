using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Services;
using Website.Shared.Models;

namespace Website.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly MessagesRepository messagesRepository;
        private readonly DiscordService discordService;

        public MessagesController(MessagesRepository messagesRepository, DiscordService discordService)
        {
            this.messagesRepository = messagesRepository;
            this.discordService = discordService;
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

            message = await messagesRepository.AddMessageAsync(message);

            await discordService.SendMessageAsync(message.Id, Request.Headers["Origin"]);

            return Ok(message);
        }

        [HttpPatch("{messageId}")]
        public async Task<IActionResult> PatchMessageAsync(int messageId)
        {
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageUserAsync(messageId, userId))
                return BadRequest();

            await messagesRepository.CloseMessageAsync(messageId, userId);
            return Ok();
        }
        
        [HttpPost("replies")]
        public async Task<IActionResult> PostMessageReplyAsync([FromBody] MessageReplyModel reply)
        {
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageUserAsync(reply.MessageId, userId))
                return BadRequest();

            reply.UserId = userId;
            reply = await messagesRepository.AddMessageReplyAsync(reply);

            await discordService.SendMessageReplyAsync(reply, Request.Headers["Origin"]);

            return Ok(reply);
        }

        [HttpPut("replies")]
        public async Task<IActionResult> PutMessageReplyAsync([FromBody] MessageReplyModel reply)
        {
            if (!await messagesRepository.IsMessageReplyUserAsync(reply.Id, int.Parse(User.Identity.Name)))
                return BadRequest();

            await messagesRepository.UpdateMessageReplyAsync(reply);
            return Ok();
        }

        [HttpDelete("replies/{replyId}")]
        public async Task<IActionResult> DeleteMessageReplyAsync(int replyId)
        {
            if (!await messagesRepository.IsMessageReplyUserAsync(replyId, int.Parse(User.Identity.Name)))
                return BadRequest();

            await messagesRepository.DeleteMessageReplyAsync(replyId);
            return Ok();
        }
    }
}
