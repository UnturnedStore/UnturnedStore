using System;
using System.ComponentModel.DataAnnotations;
using Website.Shared.Constants;

namespace Website.Shared.Models.Database
{
    public class MProductTag : IEquatable<MProductTag>
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(35)]
        public string Title { get; set; }

        [MaxLength(7)]
        [MinLength(7)]
        public string Color { get; set; } = ProductTagsConstants.DefaultColor;

        [MaxLength(7)]
        [MinLength(7)]
        public string BackgroundColor { get; set; } = ProductTagsConstants.DefaultBackgroundColor;

        public MProductTag() { }
        public MProductTag(MProductTag Tag)
        {
            Id = Tag.Id;
            Title = Tag.Title;
            Color = Tag.Color;
            BackgroundColor = Tag.BackgroundColor;
        }

        public bool Equals(MProductTag other)
        {
            return Id == other.Id;
        }
    }
}
