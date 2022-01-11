using RestoreMonarchy.PaymentGateway.Client.Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Website.Shared.Constants;

namespace Website.Shared.Models.Database
{
    public class MUser
    {
        public int Id { get; set; }
        [StringLength(25)]
        public string Name { get; set; }
        public string Role { get; set; }
        public string SteamId { get; set; }
        public int? AvatarImageId { get; set; }
        public string Color { get; set; }
        public bool IsPayPalEnabled { get; set; }
        [StringLength(255)]
        public string PayPalAddress { get; set; }
        [StringLength(3, MinimumLength = 3)]
        public string PayPalCurrency { get; set; }
        public bool IsNanoEnabled { get; set; }
        [StringLength(255)]
        public string NanoAddress { get; set; }
        [StringLength(255)]
        public string DiscordWebhookUrl { get; set; }
        [StringLength(4000)]
        public string TermsAndConditions { get; set; }
        [StringLength(4000)]
        public string Biography { get; set; }
        public DateTime CreateDate { get; set; }

        public List<MProductCustomer> Customers { get; set; }
        
        [JsonIgnore]
        public string SteamProfileUrl => "https://steamcommunity.com/profiles/" + SteamId;
        [JsonIgnore]
        public string BackgroundColor => Color ?? "#0066ff";

        public string[] GetSellerPaymentProviders(bool isMockEnabled)
        {
            List<string> paymentMethods = new();
            if (IsPayPalEnabled)
                paymentMethods.Add(PaymentProviders.PayPal);
            if (IsNanoEnabled)
                paymentMethods.Add(PaymentProviders.Nano);
            if (isMockEnabled)
                paymentMethods.Add(PaymentProviders.Mock);

            return paymentMethods.ToArray();
        }

        public static MUser FromUser(MUser user)
        {
            return new MUser()
            {
                Id = user.Id,
                SteamId = user.SteamId,
                Name = user.Name,
                AvatarImageId = user.AvatarImageId,
                Color = user.Color,
                IsPayPalEnabled = user.IsPayPalEnabled,
                PayPalAddress = user.PayPalAddress,
                PayPalCurrency = user.PayPalCurrency,
                IsNanoEnabled = user.IsNanoEnabled,
                NanoAddress = user.NanoAddress,
                Role = user.Role,
                TermsAndConditions = user.TermsAndConditions,
                Biography = user.Biography,
                DiscordWebhookUrl = user.DiscordWebhookUrl,
                CreateDate = user.CreateDate
            };
        }
    }
}
