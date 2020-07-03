using ProximaX.Sirius.Chain.Sdk.Client;
using ProximaX.Sirius.Chain.Sdk.Model.Accounts;
using ProximaX.Sirius.Chain.Sdk.Model.Blockchain;
using ProximaX.Sirius.Chain.Sdk.Model.Mosaics;
using ProximaX.Sirius.Chain.Sdk.Model.Transactions;
using ProximaX.Sirius.Chain.Sdk.Model.Transactions.Messages;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace XpxQuickTransfer
{
    class Program
    {
        public static List<Account> recipientAccounts = new List<Account>();
        public static Account senderAccount;
    
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var account = Account.CreateFromPrivateKey("B781A7162A04018CA32A5811C45A8354E317D151722E3062049EA714009A384B", NetworkType.TEST_NET);
            senderAccount = account;

            if (senderAccount != null)
            {
                Console.WriteLine("Account successfully generated.\n");
                Console.WriteLine($"{nameof(Account)} : {account}");
                Console.WriteLine("\nEnter number of recipients to create:");
                string numOfRecipients = Console.ReadLine();
                generateAccounts(Convert.ToInt32(numOfRecipients));

                Console.WriteLine("\nRecipients:\n");
                recipientAccounts.ForEach(i => Console.Write("{0}\n \n", i));
                Console.WriteLine("------------------------------------------------------------------------------------------------------------------------------------------------");

                Console.WriteLine("\nEnter amount to send.");
                string amount = Console.ReadLine();

                Console.WriteLine("\n \n");
                await CreateTransactionAsync(Convert.ToInt32(amount), recipientAccounts);

                Console.ReadLine();
            }

        }

        public static void generateAccounts(int numOfAccounts)
        {
            for (var i = 1; i <= numOfAccounts; i++)
            {
                var account = Account.GenerateNewAccount(NetworkType.TEST_NET);
                recipientAccounts.Add(account);
            }
        }

        public static async System.Threading.Tasks.Task CreateTransactionAsync(int xpxAmount, List<Account> recipients)
        {
            foreach(var recipient in recipients)
            {
         
                var client = new SiriusClient("http://bctestnet1.brimstone.xpxsirius.io:3000");
                var networkType = client.NetworkHttp.GetNetworkType().Wait();
                var mosaicToTransfer = NetworkCurrencyMosaic.CreateRelative((ulong)xpxAmount);
                var transferTransaction = TransferTransaction.Create(
                    Deadline.Create(),
                    recipient.Address,
                    new List<Mosaic>()
                    {
                        mosaicToTransfer
                    },
                    PlainMessage.Create("Transfered " + xpxAmount.ToString() + " successfully."),
                    NetworkType.TEST_NET
                );
                
               
                var generationHash = await client.BlockHttp.GetGenerationHash();
                var signedTransaction = senderAccount.Sign(transferTransaction, generationHash);
                await client.TransactionHttp.Announce(signedTransaction);
                var newAccountInfo = await client.AccountHttp.GetAccountInfo(senderAccount.Address);
                Console.WriteLine("Sent " + Convert.ToInt32(xpxAmount) + " XPX to: \n");
                Console.WriteLine($"{nameof(newAccountInfo)} : {newAccountInfo}");
            }
        }
    }
}
