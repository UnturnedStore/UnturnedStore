using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Constants
{
    public class OrderConstants
    {
        public class Status
        {
            public const string Completed = "Completed";
            public const string Pending = "Pending";
        }

        public class Methods
        {
            public const string PayPal = "PayPal";
            public const string Nano = "Nano";
        }

        
    }
}
