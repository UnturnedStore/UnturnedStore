using System.Threading.Tasks;

namespace Website.Payments.Abstractions
{
    public interface IPaymentProviders
    {
        T Get<T>() where T : IPaymentProvider;
    }
}
