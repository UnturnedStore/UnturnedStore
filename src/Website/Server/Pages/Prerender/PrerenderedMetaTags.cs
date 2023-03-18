using Website.Shared.Models.Database;

namespace Website.Server.Pages.Prerender
{
    public class PrerenderedMetaTags
    {
        public string Title { get; set; } = "Unturned Store";
        public string Description { get; set; } = "Marketplace of products for Unturned";
        public string Image { get; set; } = "/img/logo.png";

        public PrerenderedMetaTags() { }
        public PrerenderedMetaTags(MProduct Product)
        {
            Title = Product.Name + " - " + Title;
            Description = Product.Description;
            Image = "api/images/" + Product.ImageId;
        }
    }
}
