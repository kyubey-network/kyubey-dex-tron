using Andoromeda.Kyubey.TronDex.CliNet;
using System;
using System.Diagnostics;

namespace Andoromeda.Kyubey.TronDex.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var nodeAPI = new NodeApiInvoker();
            //var ct = nodeAPI.GetContractTransactionsAsync("TK6EDrMUfiRcso1uR7rNBVDjHRayKPQMoA").Result;
            //var t = nodeAPI.GetTransactionAsync("56d8123b79a05ff093bc5b55a86b13e11a0a907a31c2abcaea02561def53f50b").Result;
            var b = nodeAPI.GetBalanceAsync("TBVbLiQirADEdMsTL4WeTgNmMAgeoS16cF").Result;

            Console.ReadKey();

            var address = "TPq7HbnLXuapW9oazU6Pqsrp1cduapZhj8";
            var privateKey = "7b81cd82b28dbf9a6efb21de40fb263d83e286644ca04f910f486cb90a7a8357";
            var contractAddress = "TMWkPhsb1dnkAVNy8ej53KrFNGWy9BJrfu";
            var pwd = "Passw0rd";
            var client = new TronCliClient(@"C:\wallet-cli\", @"build\libs\wallet-cli.jar");

            //client.ImportWalletAsync(pwd, privateKey).Wait();
            client.LoginAsync("Passw0rd").Wait();
            client.TransferTRC20Async(address, contractAddress, 10).Wait();

            //client.SendCoinAsync(address, 1).Wait();
            //client.TransferAssetAsync(address, "REVOLUTION", 1).Wait();

            Console.ReadKey();
        }
    }
}
