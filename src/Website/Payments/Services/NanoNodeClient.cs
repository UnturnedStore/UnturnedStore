using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nano.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Website.Payments.Options;

namespace Website.Payments.Services
{
    public class NanoNodeClient
    {
        private readonly PaymentOptions options;
        private readonly ILogger<NanoNodeClient> logger;

        private readonly RpcClient rpcClient;

        public NanoNodeClient(IOptions<PaymentOptions> options, ILogger<NanoNodeClient> logger)
        {
            this.options = options.Value;
            this.logger = logger;

            rpcClient = new RpcClient(this.options.Providers.Nano.NodeUrl);
        }

        public Account CreateAccount(byte[] privateKey)
        {
            return Account.FromPrivateKey(privateKey);
        }

        public async Task ReceiveBlockAsync(Account account, string blockHash, Amount amount)
        {
            await rpcClient.UpdateAccountAsync(account);
            string pow = await GetPowAsync(account);

            Block receiveBlock = Block.CreateReceiveBlock(account, blockHash, amount, pow);
            await rpcClient.ProcessAsync(receiveBlock);
        }

        public async Task SendBlockAsync(Account account, string receiveAddress, Amount amount)
        {
            await rpcClient.UpdateAccountAsync(account);
            string pow = await GetPowAsync(account);

            Block sendBlock = Block.CreateSendBlock(account, receiveAddress, amount, pow);
            ProcessResponse response = await rpcClient.ProcessAsync(sendBlock);
        }

        private async Task<string> GetPowAsync(Account account)
        {
            WorkGenerateResponse workGen;

            if (account.Frontier == null)
                workGen = await rpcClient.WorkGenerateAsync(Utils.BytesToHex(account.PublicKey));
            else
                workGen = await rpcClient.WorkGenerateAsync(account.Frontier);

            return workGen.Work;
        }
    }
}
