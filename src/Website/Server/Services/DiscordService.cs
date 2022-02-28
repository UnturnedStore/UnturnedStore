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
using Website.Shared.Models.Database;

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
        private readonly IBaseUrl baseUrl;

        private IConfigurationSection config => configuration.GetSection("Discord");

        public DiscordService(IConfiguration configuration, ProductsRepository productsRepository, BranchesRepository branchesRepository, MessagesRepository messagesRepository,
            UsersRepository usersRepository, ILogger<DiscordService> logger, IBaseUrl baseUrl)
        {
            this.configuration = configuration;
            this.productsRepository = productsRepository;
            this.branchesRepository = branchesRepository;
            this.messagesRepository = messagesRepository;
            this.usersRepository = usersRepository;
            this.logger = logger;
            this.baseUrl = baseUrl;
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

        public async Task SendReviewAsync(MProductReview review, string baseUrl)
        {
            var product = await productsRepository.GetProductAsync(review.ProductId, 0);

            var eb = new EmbedBuilder();

            eb.WithColor(Color.Blue);
            eb.WithAuthor(product.Name, baseUrl + "/api/images/" + product.ImageId, baseUrl + "/products/" + product.Id);
            eb.WithFooter(review.User.Name);
            eb.WithCurrentTimestamp();

            string stars = "";
            for (int i = 1; i <= review.Rating; i++)
                stars += ":star:";
            eb.WithTitle(stars);
            eb.AddField(review.Title, review.Body);

            SendEmbed(config["SendNewReviewWebhookUrl"], eb.Build());
        }

        public async Task SendMessageReplyAsync(MMessageReply reply, string baseUrl)
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

        public async Task SendVersionUpdateAsync(MVersion version, string baseUrl)
        {
            version.Branch = await branchesRepository.GetBranchAsync(version.BranchId);
            var product = await productsRepository.GetProductAsync(version.Branch.ProductId, 0);

            var eb = new EmbedBuilder();

            eb.WithColor(Color.Blue);
            eb.WithAuthor(product.Name, baseUrl + "/api/images/" + product.ImageId, baseUrl + "/products/" + product.Id);
            eb.WithDescription($"A new version has been published on **{version.Branch.Name}** branch: **{version.Name}**");
            eb.AddField("Changelog", version.Changelog);
            eb.WithFooter(product.Seller.Name);
            eb.WithCurrentTimestamp();

            SendEmbed(config["SendPluginUpdateWebhookUrl"], eb.Build());
        }

        public void SendPurchaseNotification(MOrder order)
        {
            if (string.IsNullOrEmpty(order.Seller.DiscordWebhookUrl))
                return;

            var eb = new EmbedBuilder();
            eb.WithColor(Color.Blue);
            eb.WithTitle($"New purchase from {order.PaymentSender}");
            eb.WithCurrentTimestamp();
            eb.WithFooter(order.Buyer.Name);

            foreach (var item in order.Items)
            {
                eb.AddField(item.ProductName, $"${item.Price:N}");
            }

            SendEmbed(order.Seller.DiscordWebhookUrl, eb.Build());
        }

        public void SendProductRelease(ProductInfo product)
        {
            string url = config["SendProductReleaseWebhookUrl"];
            
            if (string.IsNullOrEmpty(url))
                return;

            var eb = new EmbedBuilder();

            eb.WithColor(Color.Blue);
            eb.WithAuthor(product.Seller.Name, baseUrl.Get("/api/images/{0}", product.Seller.AvatarImageId), baseUrl.Get("/users/{0}", product.Seller.Id));
            
            eb.WithThumbnailUrl(baseUrl.Get("api/images/{0}", product.ImageId));
            
            eb.WithTitle(product.Name);
            eb.WithUrl(baseUrl.Get("/products/{0}", product.Id));

            eb.WithDescription(product.Description);
            
            eb.AddField("Category", product.Category);
            eb.AddField("Price", product.GetPrice());

            if (!string.IsNullOrEmpty(product.GithubUrl))
            {
                eb.AddField("GitHub", product.GithubUrl);
            }

            eb.WithFooter("Check it out!");
            eb.WithCurrentTimestamp();

            SendEmbed(url, eb.Build());
        }

        public void SendApproveRequestNotification(ProductInfo product)
        {
            string url = config["AdminNotificationsWebhookUrl"];

            if (string.IsNullOrEmpty(url))
                return;

            EmbedBuilder eb = new();

            eb.WithColor(Color.Blue);
            eb.WithAuthor(product.Seller.Name, baseUrl.Get("/api/images/{0}", product.Seller.AvatarImageId), baseUrl.Get("/users/{0}", product.Seller.Id));

            eb.WithTitle(product.Name);
            eb.WithUrl(baseUrl.Get("/products/{0}", product.Id));

            eb.WithDescription($"{product.Seller.Name} has just submitted his {product.Name} plugin for approval");

            eb.WithCurrentTimestamp();

            SendEmbed(url, eb.Build());
        }
    }
}
