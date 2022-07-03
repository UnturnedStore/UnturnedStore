using System;
using System.Collections.Generic;
using System.Linq;
using Website.Shared.Models.Database;

namespace Website.Shared.Constants
{
    public class ProductTagsConstants
    {
        public const int MaximumTagsAllowed = 2;
        public const string DefaultColor = "#6c757d";
        public const string DefaultBackgroundColor = "#ebebef";

        public static string CombineTags(List<MProductTag> Tags)
        {
            return string.Join(",", Tags.Select(t => t.Id));
        }

        public static List<MProductTag> DeSerializeTags(string Tags, List<MProductTag> ProductTags)
        {
            string[] tagIds = Tags.Split(",", StringSplitOptions.RemoveEmptyEntries);
            if (ProductTags == null || tagIds.Length == 0) return new List<MProductTag>();
            
            // Doesn't auto sort the Tags and instead uses the order the User Specified
            
            List<MProductTag> tags = new List<MProductTag>();
            foreach (string tagId in tagIds)
            {
                var tag = ProductTags.FirstOrDefault(t => t.Id.ToString() == tagId);
                if (tag != null) tags.Add(tag);
            }
            
            return tags;

            //return ProductTags.Where(t => tagIds.Contains(t.Id.ToString())).ToList(); // Will auto sort the Tags by their Id
        }

        // Recommended Tags:
        //
        // Roleplay
        // Economy
        // Storage
        // Communication
        // Teleportation
        // Items
        // Vehicles
        // Animals
        // Administration
        // Server Utility
        // Miscellaneous
    }
}
