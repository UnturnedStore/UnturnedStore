using Discord;
using Discord.Rest;
using Discord.Webhook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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

        private IConfigurationSection config => configuration.GetSection("Discord");

        public DiscordService(IConfiguration configuration, ProductsRepository productsRepository, BranchesRepository branchesRepository, MessagesRepository messagesRepository,
            UsersRepository usersRepository)
        {
            this.configuration = configuration;
            this.productsRepository = productsRepository;
            this.branchesRepository = branchesRepository;
            this.messagesRepository = messagesRepository;
            this.usersRepository = usersRepository;
        }

        private async Task SendEmbedAsync(string discordWebhookUrl, Embed embed)
        {
            using (var client = new DiscordWebhookClient(discordWebhookUrl))
            {
                await client.SendMessageAsync(embeds: new Embed[] { embed });
            }
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

            await SendEmbedAsync(discordWebhookUrl, eb.Build());
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

            await SendEmbedAsync(discordWebhookUrl, eb.Build());
        }

        public async Task SendPluginUpdateAsync(PluginModel plugin, string baseUrl)
        {
            plugin.Branch = await branchesRepository.GetBranchAsync(plugin.BranchId);
            var product = await productsRepository.GetProductAsync(plugin.Branch.ProductId, 0);

            var eb = new EmbedBuilder();

            eb.WithColor(Color.Blue);
            eb.WithAuthor(product.Name, baseUrl + "/api/images/" + product.ImageId, baseUrl + "/products/" + product.Id);
            eb.WithDescription($"A new version has been published on **{plugin.Branch.Name}** branch: **{plugin.Version}**");
            eb.AddField("Changelog", plugin.Changelog);
            eb.WithFooter(product.Seller.Name);
            eb.WithCurrentTimestamp();

            await SendEmbedAsync(config["SendPluginUpdateWebhookUrl"], eb.Build());
        }
    }
}
