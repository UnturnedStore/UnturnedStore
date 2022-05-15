using Discord;
using Discord.Webhook;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
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

        private IConfigurationSection config => configuration.GetSection("Discord");

        private void SendEmbed(string discordWebhookUrl, Embed embed)
        {
            Task.Run(async () =>
            {
                try
                {
                    using (DiscordWebhookClient client = new DiscordWebhookClient(discordWebhookUrl))
                    {
                        await client.SendMessageAsync(embeds: new Embed[] 
                        { 
                            embed
                        });
                    }
                } 
                catch (Exception e)
                {
                    logger.LogError(e, "An exception occurated when sending discord webhook");
                }                
            });
        }

        public async Task SendReviewAsync(MProductReview productReview)
        {
            MProduct product = await productsRepository.GetProductAsync(productReview.ProductId, 0);

            string iconUrl = baseUrl.Get("/api/images/{0}", product.ImageId);
            string productUrl = baseUrl.Get("/products/{0}", product.Id);

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithAuthor(product.Name, iconUrl, productUrl);
            embedBuilder.WithFooter(productReview.User.Name);
            embedBuilder.WithCurrentTimestamp();

            string stars = string.Empty;
            for (int i = 1; i <= productReview.Rating; i++)
            {
                stars += ":star:";
            }
            embedBuilder.WithTitle(stars);
            embedBuilder.AddField(productReview.Title, productReview.Body);

            SendEmbed(config["SendNewReviewWebhookUrl"], embedBuilder.Build());
        }

        public async Task SendMessageReplyAsync(MMessageReply reply)
        {
            MMessage message = await messagesRepository.GetMessageAsync(reply.MessageId);
            UserInfo senderUserInfo = message.FromUserId == reply.UserId ? message.FromUser : message.ToUser;
            string discordWebhookUrl = await usersRepository.GetUserDiscordWebhookUrl(message.FromUserId == reply.UserId ? message.ToUserId : message.FromUserId);

            if (string.IsNullOrWhiteSpace(discordWebhookUrl))
            {
                return;
            }

            string messageUrl = baseUrl.Get("/messages/{0}", message.Id);
            string title = $"New Reply Message #{message.Id}";
            string description = $"{senderUserInfo.Name} sent a new reply to message: **{message.Title}**";

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithUrl(messageUrl);
            embedBuilder.WithTitle(title);
            embedBuilder.WithDescription(description);
            embedBuilder.WithFooter(senderUserInfo.Name);
            embedBuilder.WithCurrentTimestamp();

            SendEmbed(discordWebhookUrl, embedBuilder.Build());
        }

        public async Task SendMessageAsync(int messageId)
        {
            MMessage message = await messagesRepository.GetMessageAsync(messageId);
            string discordWebhookUrl = await usersRepository.GetUserDiscordWebhookUrl(message.ToUserId);

            if (string.IsNullOrWhiteSpace(discordWebhookUrl))
            {
                return;
            }

            string url = baseUrl.Get("/messages/{0}", message.Id);
            string title = $"New Message #{message.Id}";
            string description = $"{message.FromUser.Name} sent you a new message: **{message.Title}**";

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithUrl(url);
            embedBuilder.WithTitle(title);
            embedBuilder.WithDescription(description);
            embedBuilder.WithFooter(message.FromUser.Name);
            embedBuilder.WithCurrentTimestamp();

            SendEmbed(discordWebhookUrl, embedBuilder.Build());
        }

        public async Task SendVersionUpdateAsync(MVersion version)
        {
            version.Branch = await branchesRepository.GetBranchAsync(version.BranchId);
            MProduct product = await productsRepository.GetProductAsync(version.Branch.ProductId, 0);

            string iconUrl = baseUrl.Get("/api/images/{0}", product.ImageId);
            string url = baseUrl.Get("/products/{0}", product.Id);
            string description 
                = $"A new version has been published on **{version.Branch.Name}** branch: **{version.Name}**";

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithAuthor(product.Name, iconUrl, url);
            embedBuilder.WithDescription(description);
            embedBuilder.AddField("Changelog", version.Changelog);
            embedBuilder.WithFooter(product.Seller.Name);
            embedBuilder.WithCurrentTimestamp();

            SendEmbed(config["SendPluginUpdateWebhookUrl"], embedBuilder.Build());
        }

        public void SendPurchaseNotification(MOrder order)
        {
            if (string.IsNullOrWhiteSpace(order.Seller.DiscordWebhookUrl))
            {
                return;
            }

            string title = $"New purchase from {order.PaymentSender}";

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithTitle(title);
            embedBuilder.WithCurrentTimestamp();
            embedBuilder.WithFooter(order.Buyer.Name);

            foreach (MOrderItem orderItem in order.Items)
            {
                embedBuilder.AddField(orderItem.ProductName, $"${orderItem.Price:N}");
            }

            SendEmbed(order.Seller.DiscordWebhookUrl, embedBuilder.Build());
        }

        public void SendProductRelease(ProductInfo productInfo)
        {
            string url = config["SendProductReleaseWebhookUrl"];
            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            string authorIconUrl = baseUrl.Get("/api/images/{0}", productInfo.Seller.AvatarImageId);
            string authorUrl = baseUrl.Get("/users/{0}", productInfo.Seller.Id);
            string thumbnailUrl = baseUrl.Get("api/images/{0}", productInfo.ImageId);
            string titleUrl = baseUrl.Get("/products/{0}", productInfo.Id);
            string footer = "Check it out!";

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithAuthor(productInfo.Seller.Name, authorIconUrl, authorUrl);            
            embedBuilder.WithThumbnailUrl(thumbnailUrl);            
            embedBuilder.WithTitle(productInfo.Name);
            embedBuilder.WithUrl(titleUrl);
            embedBuilder.WithDescription(productInfo.Description);
            
            embedBuilder.AddField("Category", productInfo.Category);
            embedBuilder.AddField("Price", productInfo.GetPrice());

            if (!string.IsNullOrEmpty(productInfo.GithubUrl))
            {
                embedBuilder.AddField("GitHub", productInfo.GithubUrl);
            }

            embedBuilder.WithFooter(footer);
            embedBuilder.WithCurrentTimestamp();

            SendEmbed(url, embedBuilder.Build());
        }

        public void SendApproveRequestNotification(ProductInfo productInfo)
        {
            string url = config["AdminNotificationsWebhookUrl"];
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            string authorIconUrl = baseUrl.Get("/api/images/{0}", productInfo.Seller.AvatarImageId);
            string authorUrl = baseUrl.Get("/users/{0}", productInfo.Seller.Id);
            string titleUrl = baseUrl.Get("/products/{0}", productInfo.Id);
            string description 
                = $"{productInfo.Seller.Name} has just submitted his {productInfo.Name} plugin for approval";

            EmbedBuilder embedBuilder = new();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithAuthor(productInfo.Seller.Name, authorIconUrl, authorUrl);
            embedBuilder.WithTitle(productInfo.Name);
            embedBuilder.WithUrl(titleUrl);
            embedBuilder.WithDescription(description);
            embedBuilder.WithCurrentTimestamp();

            SendEmbed(url, embedBuilder.Build());
        }
    }
}
