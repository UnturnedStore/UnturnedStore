using System;

namespace Website.Shared.Models.Database
{
    public class MCustomerServer
    {
        public int CustomerId { get; set; }
        public string ServerName { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int TimesLoaded { get; set; }
        public DateTime UpdateDate { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
