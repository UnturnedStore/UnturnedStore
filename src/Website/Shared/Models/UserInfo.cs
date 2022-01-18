using System;

namespace Website.Shared.Models
{
    public class UserInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string SteamId { get; set; }
        public int? AvatarImageId { get; set; }
        public string Color { get; set; }
        public DateTime CreateDate { get; set; }

        public string AvatarUrl() => AvatarImageId.HasValue ? 
            $"api/images/{AvatarImageId}" : "/img/profiles/default_avatar.png";
    }
}
