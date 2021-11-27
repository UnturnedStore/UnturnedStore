using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nano.Net;
using Nano.Net.WebSockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Website.Payments.Options;
using Website.Shared.Models;

namespace Website.Payments.Services
{
    public class NanoHostedService : IHostedService, IDisposable
    {
        private readonly PaymentOptions options;
        private readonly ILogger<NanoHostedService> logger;
        private readonly NanoPaymentStore paymentStore;
        private readonly NanoNodeClient nodeClient;

        private readonly NanoWebSocketClient webSocketClient;        

        public NanoHostedService(IOptions<PaymentOptions> options, ILogger<NanoHostedService> logger, NanoPaymentStore paymentStore, NanoNodeClient nodeClient)
        {
            this.options = options.Value;
            this.paymentStore = paymentStore;
            this.nodeClient = nodeClient;

            webSocketClient = new NanoWebSocketClient(this.options.Providers.Nano.WebSocketUrl);
            webSocketClient.Subscribe(new ConfirmationTopic());
            webSocketClient.Confirmation += WebSocketClient_Confirmation;
        }

        private async void WebSocketClient_Confirmation(NanoWebSocketClient client, ConfirmationTopicMessage topicMessage)
        {
            if (topicMessage.Message.Block.Subtype != "send")
            {
                return;
            }

            string receiveAddress = topicMessage.Message.Block.LinkAsAccount;

            MNanoPayment payment = paymentStore.GetNanoPaymentByReceiver(receiveAddress);
            if (payment == null)
            {
                return;
            }

            string senderAddress = topicMessage.Message.Block.Account;

            Account account = Account.FromPrivateKey(payment.ReceivePrivateKey);
            string blockHash = topicMessage.Message.Hash;
            Amount amount = Amount.FromRaw(topicMessage.Message.Amount);

            await nodeClient.ReceiveBlockAsync(account, blockHash, amount);
            
            if (amount.Nano == payment.Amount)
            {
                payment.SendAddress = senderAddress;
                payment.Amount = (decimal)amount.Nano;
                await paymentStore.ReceivePaymentAsync(payment);

                // Send nano to seller if the amount is valid
                await nodeClient.SendBlockAsync(account, payment.SellerAddress, amount);

            }
            else
            {
                // Refund nano to sender if amount is invalid
                await nodeClient.SendBlockAsync(account, senderAddress, amount);                
            }
        }

        // Start and Stop methods

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await paymentStore.ReloadPendingPaymentsAsync();
            await webSocketClient.Start();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await webSocketClient.Stop();
        }

        public void Dispose()
        {
            webSocketClient.Dispose();
        }
    }
}
