using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Data.Repositories;
using Website.Shared.Models;

namespace Website.Payments.Services
{
    public class NanoPaymentStore
    {
        private readonly NanoRepository nanoRepository;

        private List<MNanoPayment> pendingPayments;

        public NanoPaymentStore(NanoRepository nanoRepository)
        {
            this.nanoRepository = nanoRepository;
            pendingPayments = new List<MNanoPayment>();
        }

        public async Task ReceivePaymentAsync(MNanoPayment payment)
        {
            await nanoRepository.UpdatePaymentAsync(payment);
            pendingPayments.Remove(payment);
        }

        public async Task ReloadPendingPaymentsAsync()
        {
            IEnumerable<MNanoPayment> payments = await nanoRepository.GetPendingPaymentsAsync();
            pendingPayments = payments.ToList();
        }

        public async Task AddNanoPaymentAsync(MNanoPayment payment)
        {
            await nanoRepository.AddPaymentAsync(payment);
        }

        public MNanoPayment GetNanoPaymentByReceiver(string receiveAddress)
        {
            return pendingPayments.FirstOrDefault(x => x.ReceiveAddress == receiveAddress);
        }
    }
}
