using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Server.Services;
using Website.Shared.Models.Database;

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
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageUserAsync(messageId, userId))
            {
                return BadRequest();
            }

            MMessage message = await messagesRepository.GetMessageAsync(messageId);

            message.Read = await messagesRepository.GetMessageReadAsync(messageId, userId);

            return Ok(message);
        }

        [HttpPost]
        public async Task<IActionResult> PostMessageAsync([FromBody] MMessage message)
        {
            int userId = int.Parse(User.Identity.Name);
            message.FromUserId = userId;
            foreach (var reply in message.Replies)
            {
                reply.UserId = userId;
            }

            message = await messagesRepository.AddMessageAsync(message);

            await discordService.SendMessageAsync(message.Id);

            return Ok(message);
        }

        [HttpPatch("{messageId}")]
        public async Task<IActionResult> PatchMessageAsync(int messageId)
        {
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageUserAsync(messageId, userId))
            {
                return BadRequest();
            }

            await messagesRepository.CloseMessageAsync(messageId, userId);
            return Ok();
        }
        
        [HttpPost("replies")]
        public async Task<IActionResult> PostMessageReplyAsync([FromBody] MMessageReply reply)
        {
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageUserAsync(reply.MessageId, userId))
            {
                return BadRequest();
            }

            reply.UserId = userId;
            reply = await messagesRepository.AddMessageReplyAsync(reply);

            await discordService.SendMessageReplyAsync(reply);

            return Ok(reply);
        }

        [HttpPut("replies")]
        public async Task<IActionResult> PutMessageReplyAsync([FromBody] MMessageReply reply)
        {
            if (!await messagesRepository.IsMessageReplyUserAsync(reply.Id, int.Parse(User.Identity.Name)))
            {
                return BadRequest();
            }

            await messagesRepository.UpdateMessageReplyAsync(reply);
            return Ok();
        }

        [HttpDelete("replies/{replyId}")]
        public async Task<IActionResult> DeleteMessageReplyAsync(int replyId)
        {
            if (!await messagesRepository.IsMessageReplyUserAsync(replyId, int.Parse(User.Identity.Name)))
            {
                return BadRequest();
            }

            await messagesRepository.DeleteMessageReplyAsync(replyId);
            return Ok();
        }

        [HttpGet("read")]
        public async Task<IActionResult> GetMessageReadAsync()
        {
            int userId = int.Parse(User.Identity.Name);
            return Ok(await messagesRepository.GetNewMessagesAsync(userId));
        }

        [HttpPost("read")]
        public async Task<IActionResult> PostMessageReadAsync([FromBody] MMessageRead read)
        {
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageUserAsync(read.MessageId, userId))
            {
                return BadRequest();
            }

            read.UserId = userId;
            read = await messagesRepository.AddMessageReadAsync(read);
            
            return Ok(read);
        }

        [HttpPut("read")]
        public async Task<IActionResult> PutMessageReadAsync([FromBody] MMessageRead read)
        {
            int userId = int.Parse(User.Identity.Name);
            if (!await messagesRepository.IsMessageUserAsync(read.MessageId, userId))
            {
                return BadRequest();
            }

            await messagesRepository.UpdateMessageReadAsync(read);
            return Ok();
        }
    }
}
