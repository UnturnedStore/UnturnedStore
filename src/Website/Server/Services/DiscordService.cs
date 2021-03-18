using Discord;
using Discord.Rest;
using Discord.Webhook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models;

namespace Website.Server.Services
{
    public class DiscordService
    {
        private readonly IConfiguration configuration;
        private readonly ProductsRepository productsRepository;
        private readonly BranchesRepository branchesRepository;
        private readonly MessagesRepository messagesRepository;
        private readonly UsersRepository usersRepository;
        private readonly ILogger<DiscordService> logger;

        private IConfigurationSection config => configuration.GetSection("Discord");

        public DiscordService(IConfiguration configuration, ProductsRepository productsRepository, BranchesRepository branchesRepository, MessagesRepository messagesRepository,
            UsersRepository usersRepository, ILogger<DiscordService> logger)
        {
            this.configuration = configuration;
            this.productsRepository = productsRepository;
            this.branchesRepository = branchesRepository;
            this.messagesRepository = messagesRepository;
            this.usersRepository = usersRepository;
            this.logger = logger;
        }

        private void SendEmbed(string discordWebhookUrl, Embed embed)
        {
            Task.Run(async () =>
            {
                try
                {
                    using (var client = new DiscordWebhookClient(discordWebhookUrl))
                    {
                        await client.SendMessageAsync(embeds: new Embed[] { embed });
                    }
                } catch (Exception e)
                {
                    logger.LogError(e, "An exception occurated when sending discord webhook");
                }                
            });
        }

        public async Task SendMessageReplyAsync(MessageReplyModel reply, string baseUrl)
        {
            var msg = await messagesRepository.GetMessageAsync(reply.MessageId);

            var sender = msg.FromUserId == reply.UserId ? msg.FromUser : msg.ToUser;

            var discordWebhookUrl = await usersRepository.GetUserDiscordWebhookUrl(msg.FromUserId == reply.UserId ? msg.ToUserId : msg.FromUserId);

            if (string.IsNullOrEmpty(discordWebhookUrl))
                return;

            var eb = new EmbedBuilder();

            eb.WithColor(Color.Blue);
            eb.WithUrl(baseUrl + "/messages/" + msg.Id);
            eb.WithTitle($"New Reply Message #{msg.Id}");
            eb.WithDescription($"{sender.Name} sent a new reply to message: **{msg.Title}**");
            eb.WithFooter(sender.Name);
            eb.WithCurrentTimestamp();

            SendEmbed(discordWebhookUrl, eb.Build());
        }

        public async Task SendMessageAsync(int messageId, string baseUrl)
        {
            var msg = await messagesRepository.GetMessageAsync(messageId);
            var discordWebhookUrl = await usersRepository.GetUserDiscordWebhookUrl(msg.ToUserId);

            if (string.IsNullOrEmpty(discordWebhookUrl))
                return;

            var eb = new EmbedBuilder();

            eb.WithColor(Color.Blue);
            eb.WithUrl(baseUrl + "/messages/" + msg.Id);
            eb.WithTitle($"New Message #{msg.Id}");
            eb.WithDescription($"{msg.FromUser.Name} sent you a new message: **{msg.Title}**");
            eb.WithFooter(msg.FromUser.Name);
            eb.WithCurrentTimestamp();

            SendEmbed(discordWebhookUrl, eb.Build());
        }

        public async Task SendPluginUpdateAsync(VersionModel plugin, string baseUrl)
        {
            plugin.Branch = await branchesRepository.GetBranchAsync(plugin.BranchId);
            var product = await productsRepository.GetProductAsync(plugin.Branch.ProductId, 0);

            var eb = new EmbedBuilder();

            eb.WithColor(Color.Blue);
            eb.WithAuthor(product.Name, baseUrl + "/api/images/" + product.ImageId, baseUrl + "/products/" + product.Id);
            eb.WithDescription($"A new version has been published on **{plugin.Branch.Name}** branch: **{plugin.Name}**");
            eb.AddField("Changelog", plugin.Changelog);
            eb.WithFooter(product.Seller.Name);
            eb.WithCurrentTimestamp();

            SendEmbed(config["SendPluginUpdateWebhookUrl"], eb.Build());
        }
    }
}
