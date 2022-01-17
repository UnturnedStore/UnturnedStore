using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Website.Payments.Abstractions;

namespace Website.Payments
{
    public class PaymentProviders : IPaymentProviders
    {
        private readonly IEnumerable<IPaymentProvider> providers;

        public PaymentProviders(IEnumerable<IPaymentProvider> providers)
        {
            this.providers = providers ?? throw new ArgumentNullException(nameof(providers));
        }

        public T Get<T>() where T : IPaymentProvider
        {
            IPaymentProvider result = providers.FirstOrDefault(p => p.GetType().Equals(typeof(T)));

            if (result == null)
                throw new InvalidOperationException($"Payment provider for {typeof(T)} not registered.");

            return (T)result;
        }
    }
}
