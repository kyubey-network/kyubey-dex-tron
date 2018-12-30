using Andoromeda.Kyubey.TronDex.CliNet;
using System;
using System.Diagnostics;

namespace Andoromeda.Kyubey.TronDex.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var address = "TPq7HbnLXuapW9oazU6Pqsrp1cduapZhj8";
            var privateKey = "7b81cd82b28dbf9a6efb21de40fb263d83e286644ca04f910f486cb90a7a8357";
            var pwd = "Passw0rd";
            var client = new TronCliClient(@"C:\wallet-cli\", @"build\libs\wallet-cli.jar");

            //client.ImportWalletAsync(pwd, privateKey).Wait();
            client.LoginAsync("Passw0rd").Wait();

            //client.SendCoinAsync(address, 1).Wait();
            //client.TransferAssetAsync(address, "REVOLUTION", 1).Wait();

            Console.ReadKey();
        }
    }
}
