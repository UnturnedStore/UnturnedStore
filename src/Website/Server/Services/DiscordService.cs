using Discord;
using Discord.Webhook;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private IConfigurationSection config => configuration.GetSection("Discord");

        public DiscordService(IConfiguration configuration, ProductsRepository productsRepository, BranchesRepository branchesRepository)
        {
            this.configuration = configuration;
            this.productsRepository = productsRepository;
            this.branchesRepository = branchesRepository;
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

            using (var client = new DiscordWebhookClient(config["SendPluginUpdateWebhookUrl"]))
            {
                await client.SendMessageAsync(embeds: new Embed[] { eb.Build() });
            }
        }
    }
}
