using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Website.Shared.Models
{
    public class MUser
    {
        public int Id { get; set; }
        [MaxLength(25)]
        public string Name { get; set; }
        public string Role { get; set; }
        public string SteamId { get; set; }
        public int? AvatarImageId { get; set; }
        public string Color { get; set; }
        public byte[] Avatar { get; set; }
        [MaxLength(255)]
        public string PayPalEmail { get; set; }
        [MinLength(3)]
        [MaxLength(3)]
        public string PayPalCurrency { get; set; }
        [MaxLength(255)]
        public string DiscordWebhookUrl { get; set; }
        [MaxLength(4000)]
        public string TermsAndConditions { get; set; }
        [MaxLength(4000)]
        public string Biography { get; set; }
        public DateTime CreateDate { get; set; }

        public List<MProductCustomer> Customers { get; set; }
        
        [JsonIgnore]
        public string SteamProfileUrl => "https://steamcommunity.com/profiles/" + SteamId;
        [JsonIgnore]
        public string BackgroundColor => Color ?? "#0066ff";
        

        public static MUser FromUser(MUser user)
        {
            return new MUser()
            {
                Id = user.Id,
                SteamId = user.SteamId,
                Name = user.Name,
                AvatarImageId = user.AvatarImageId,
                Color = user.Color,
                PayPalCurrency = user.PayPalCurrency,
                PayPalEmail = user.PayPalEmail,
                Role = user.Role,
                TermsAndConditions = user.TermsAndConditions,
                Biography = user.Biography,
                DiscordWebhookUrl = user.DiscordWebhookUrl,
                CreateDate = user.CreateDate
            };
        }
    }
}
