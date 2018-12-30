using System;
using System.Diagnostics;

namespace Andoromeda.Kyubey.TronDex.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var privateKey = "7b81cd82b28dbf9a6efb21de40fb263d83e286644ca04f910f486cb90a7a8357";
            var pwd = "Passw0rd";


            Process p = new Process();
            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = @"java";
            pi.Arguments = @" -jar build\libs\wallet-cli.jar";
            pi.WorkingDirectory = @"C:\wallet-cli\";
            pi.RedirectStandardInput = true;
            pi.RedirectStandardOutput = true;
            pi.CreateNoWindow = true;
            pi.UseShellExecute = false;

            p.StartInfo = pi;
            p.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
            {
                Console.WriteLine(e.Data);
                if (e.Data.Contains("Please type one of the following commands to proceed."))
                {
                    Console.WriteLine("Login");
                    p.StandardInput.WriteLine("Login");
                }

                if (e.Data.Contains("Please input your password.") || e.Data.Contains("Please input password.") || e.Data.Contains("Please input password again."))
                {
                    Console.WriteLine(pwd);
                    p.StandardInput.WriteLine(pwd);
                }

                //if (e.Data.Contains("Login successful !!!"))
                //{
                //    Console.WriteLine("TransferAsset TPq7HbnLXuapW9oazU6Pqsrp1cduapZhj8 REVOLUTION 1");
                //    p.StandardInput.WriteLine("TransferAsset TPq7HbnLXuapW9oazU6Pqsrp1cduapZhj8 REVOLUTION 1");
                //}

                //if (e.Data.Contains("Login successful !!!"))
                //{
                //    Console.WriteLine("SendCoin TPq7HbnLXuapW9oazU6Pqsrp1cduapZhj8 1");
                //    p.StandardInput.WriteLine("SendCoin TPq7HbnLXuapW9oazU6Pqsrp1cduapZhj8 1");
                //}

                //if (e.Data.Contains("Please confirm that you want to continue enter y or Y, else any other."))
                //{
                //    Console.WriteLine("y");
                //    p.StandardInput.WriteLine("y");
                //}
            };
            p.Start();
            p.BeginOutputReadLine();
            p.WaitForExit();
            Console.ReadKey();
        }
    }
}
