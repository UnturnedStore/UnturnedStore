using System;

namespace Website.Shared.Models.Product
{
    public class ProductLicense
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public Guid LicenseKey { get; set; }
    }
}
