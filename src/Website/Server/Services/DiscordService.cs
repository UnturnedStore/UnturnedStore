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

        public async Task SendReviewAsync(MProductReview productReview, string baseUrl)
        {
            MProduct product = await productsRepository.GetProductAsync(productReview.ProductId, 0);

            string iconUrl = new StringBuilder()
                .Append(baseUrl)
                .Append("/api/images/")
                .Append(product.ImageId)
                .ToString();

            string url = new StringBuilder()
                 .Append(baseUrl)
                 .Append("/products/")
                 .Append(product.Id)
                 .ToString();

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithAuthor(product.Name, iconUrl, url);
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

        public async Task SendMessageReplyAsync(MMessageReply reply, string baseUrl)
        {
            MMessage message = await messagesRepository.GetMessageAsync(reply.MessageId);
            UserInfo senderUserInfo = message.FromUserId == reply.UserId ? message.FromUser : message.ToUser;
            string discordWebhookUrl = await usersRepository.GetUserDiscordWebhookUrl(message.FromUserId == reply.UserId ? message.ToUserId : message.FromUserId);

            if (string.IsNullOrWhiteSpace(discordWebhookUrl))
            {
                return;
            }

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithUrl(new StringBuilder()
                .Append(baseUrl)
                .Append("/messages/")
                .Append(message.Id)
                .ToString());
            embedBuilder.WithTitle($"New Reply Message #{message.Id}");
            embedBuilder.WithDescription($"{senderUserInfo.Name} sent a new reply to message: **{message.Title}**");
            embedBuilder.WithFooter(senderUserInfo.Name);
            embedBuilder.WithCurrentTimestamp();

            SendEmbed(discordWebhookUrl, embedBuilder.Build());
        }

        public async Task SendMessageAsync(int messageId, string baseUrl)
        {
            MMessage message = await messagesRepository.GetMessageAsync(messageId);
            string discordWebhookUrl = await usersRepository.GetUserDiscordWebhookUrl(message.ToUserId);

            if (string.IsNullOrWhiteSpace(discordWebhookUrl))
            {
                return;
            }

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithUrl(new StringBuilder()
                .Append(baseUrl)
                .Append("/messages/")
                .Append(message.Id)
                .ToString());
            embedBuilder.WithTitle($"New Message #{message.Id}");
            embedBuilder.WithDescription($"{message.FromUser.Name} sent you a new message: **{message.Title}**");
            embedBuilder.WithFooter(message.FromUser.Name);
            embedBuilder.WithCurrentTimestamp();

            SendEmbed(discordWebhookUrl, embedBuilder.Build());
        }

        public async Task SendVersionUpdateAsync(MVersion version, string baseUrl)
        {
            version.Branch = await branchesRepository.GetBranchAsync(version.BranchId);
            MProduct product = await productsRepository.GetProductAsync(version.Branch.ProductId, 0);

            string iconUrl = new StringBuilder()
                .Append(baseUrl)
                .Append("/api/images/")
                .Append(product.ImageId)
                .ToString();

            string url = new StringBuilder()
                .Append(baseUrl)
                .Append("/products/")
                .Append(product.Id)
                .ToString();

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithAuthor(product.Name, iconUrl, url);
            embedBuilder.WithDescription($"A new version has been published on **{version.Branch.Name}** branch: **{version.Name}**");
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

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithTitle($"New purchase from {order.PaymentSender}");
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

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithAuthor(productInfo.Seller.Name, baseUrl.Get("/api/images/{0}", productInfo.Seller.AvatarImageId), baseUrl.Get("/users/{0}", productInfo.Seller.Id));
            
            embedBuilder.WithThumbnailUrl(baseUrl.Get("api/images/{0}", productInfo.ImageId));
            
            embedBuilder.WithTitle(productInfo.Name);
            embedBuilder.WithUrl(baseUrl.Get("/products/{0}", productInfo.Id));

            embedBuilder.WithDescription(productInfo.Description);
            
            embedBuilder.AddField("Category", productInfo.Category);
            embedBuilder.AddField("Price", productInfo.GetPrice());

            if (!string.IsNullOrEmpty(productInfo.GithubUrl))
            {
                embedBuilder.AddField("GitHub", productInfo.GithubUrl);
            }

            embedBuilder.WithFooter("Check it out!");
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

            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.WithColor(Color.Blue);
            embedBuilder.WithAuthor(productInfo.Seller.Name, baseUrl.Get("/api/images/{0}", productInfo.Seller.AvatarImageId), baseUrl.Get("/users/{0}", productInfo.Seller.Id));

            embedBuilder.WithTitle(productInfo.Name);
            embedBuilder.WithUrl(baseUrl.Get("/products/{0}", productInfo.Id));

            embedBuilder.WithDescription($"{productInfo.Seller.Name} has just submitted his {productInfo.Name} plugin for approval");

            embedBuilder.WithCurrentTimestamp();

            SendEmbed(url, embedBuilder.Build());
        }
    }
}
