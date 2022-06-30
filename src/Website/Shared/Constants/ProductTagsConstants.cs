using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Shared.Constants
{
    public class ProductTagsConstants
    {
        public static string CombineTags(params string[] Tags)
        {
            return string.Join(",", Tags);
        }

        public static MProductTag[] DeSerializeTags(string Tags)
        {
            if (string.IsNullOrEmpty(Tags)) return Array.Empty<MProductTag>();

            List<MProductTag> DeSerializedTags = new List<MProductTag>();
            foreach (string tag in Tags.Split(","))
            {
                var newTag = ProductTags.FirstOrDefault(t => t.Title == tag);
                if (newTag != null) DeSerializedTags.Add(newTag);
            }

            return DeSerializedTags.ToArray();
        }

        public static MProductTag[] ProductTags => new MProductTag[] {
            new MProductTag()
            {
                Title = "Roleplay"
            },
            new MProductTag()
            {
                Title = "Economy"
            },
            new MProductTag()
            {
                Title = "Storage"
            },
            new MProductTag()
            {
                Title = "Communication"
            },
            new MProductTag()
            {
                Title = "Teleportation"
            },
            new MProductTag()
            {
                Title = "Items"
            },
            new MProductTag()
            {
                Title = "Vehicles"
            },
            new MProductTag()
            {
                Title = "Animals"
            },
            new MProductTag()
            {
                Title = "Administration"
            },
            new MProductTag()
            {
                Title = "Server Utility"
            },
            new MProductTag()
            {
                Title = "Miscellaneous"
            }
        };
    }
}
