using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Website.Shared.Models
{
    public class MNanoPayment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string SellerAddress { get; set; }
        public string ReceiveAddress { get; set; }
        public byte[] ReceivePrivateKey { get; set; }
        public string SendAddress { get; set; }
        public decimal Amount { get; set; }
        public bool IsReceived { get; set; }
        public DateTime CreateDate { get; set; }
    }
}
