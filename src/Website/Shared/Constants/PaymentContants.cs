using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Constants
{
    public class PaymentContants
    {
        public const string PayPal = "PayPal";
        public const string PayPalSandobxUrl = "https://www.sandbox.paypal.com/cgi-bin/";
        public const string PayPalUrl = "https://www.paypal.com/cgi-bin/";

        public static string GetPayPalUrl(bool useSandbox) => useSandbox ? PayPalSandobxUrl : PayPalUrl;

        public const string PendingStatus = "Pending";
        public const string CompletedStatus = "Completed";
    }
}
