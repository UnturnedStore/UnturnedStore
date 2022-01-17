using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Shared.Models;

namespace Website.Payments.Models
{
    public class ValidatePaymentResult
    {
        public PaymentStatus Status { get; set; }
        public MOrder Order { get; set; }
        public string ErrorMessage { get; set; }
    }
}
