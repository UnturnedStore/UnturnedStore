using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Payments.Models
{
    public enum PaymentStatus
    {
        Valid,
        NotVerified,
        InvalidCustom,
        ReceiverMismatch,
        CurrencyMismatch,
        AmountMismatch
    }
}
