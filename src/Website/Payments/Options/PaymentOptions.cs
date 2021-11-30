namespace Website.Payments.Options
{
    public class PaymentOptions
    {
        public const string Payment = "Payment";

        public int HoursToExpire { get; set; }
        public ProvidersOptions Providers { get; set; }

        public class ProvidersOptions
        {
            public PayPalOptions PayPal { get; set; }
            public NanoOptions Nano { get; set; }
        }

        public class PayPalOptions
        {
            public bool IsEnabled { get; set; }
            public bool UseSandbox { get; set; }
        }

        public class NanoOptions
        {
            public bool IsEnabled { get; set; }
            public string NodeUrl { get; set; }
        }
    }
}
