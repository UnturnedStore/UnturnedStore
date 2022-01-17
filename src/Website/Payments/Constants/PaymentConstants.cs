using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Payments.Constants
{
    public static class PaymentConstants
    {
        public const string LogoUrl = "/img/paypal-logo.png";
        public static string GetLogoUrl(string baseUrl) => baseUrl.TrimEnd('/') + LogoUrl; 

        public static class Providers
        {
            public static class Nano
            {

            }

            public static class PayPal
            {
                public const string Name = "PayPal";
                public const string NotifyUrl = "/api/orders/paypal";

                public const string PayPalSandobxUrl = "https://www.sandbox.paypal.com/cgi-bin/";
                public const string PayPalUrl = "https://www.paypal.com/cgi-bin/";

                public static string GetNotifyUrl(string baseUrl) => baseUrl.TrimEnd('/') + NotifyUrl;
                public static string GetPayPalUrl(bool useSandbox) => useSandbox ? PayPalSandobxUrl : PayPalUrl;

                public const string PendingStatus = "Pending";
                public const string CompletedStatus = "Completed";
            }
        }
    }
}
