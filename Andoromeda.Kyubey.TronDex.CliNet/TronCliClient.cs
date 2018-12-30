using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class TronCliClient : IDisposable
    {
        private Process process;
        private Dictionary<string, Func<string, bool?>> callbacks;
        private string current;
        private string password;
        private TaskCompletionSource<bool> currentTask;
        private TaskCompletionSource<bool> initTask;

        public TronCliClient(string workingDirectory, string walletJarPath)
        {
            callbacks = new Dictionary<string, Func<string, bool?>>();
            process = new Process();
            initTask = new TaskCompletionSource<bool>();

            ProcessStartInfo pi = new ProcessStartInfo();
            pi.FileName = @"java";
            pi.Arguments = $@" -jar {walletJarPath}";
            pi.RedirectStandardInput = true;
            pi.RedirectStandardOutput = true;
            pi.CreateNoWindow = true;
            pi.UseShellExecute = false;
            pi.WorkingDirectory = workingDirectory;
            process.StartInfo = pi;
            process.OutputDataReceived += OnOutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();
            initTask.Task.Wait();
        }

        private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
            if (e.Data.Contains("Please type one of the following commands to proceed."))
            {
                initTask.SetResult(true);
            }
            else if (current != null && callbacks.ContainsKey(current))
            {
                var result = callbacks[current](e.Data);
                if (result.HasValue)
                {
                    currentTask?.SetResult(result.Value);
                    current = null;
                }
            }
        }

        private Task<bool> InvokeAsync(string command, string alias, Func<string, bool?> callback)
        {
            if (!callbacks.ContainsKey(alias))
            {
                callbacks.Add(alias, callback);
            }
            current = alias;
            process.StandardInput.WriteLine(command);
            Console.WriteLine(command);
            currentTask = new TaskCompletionSource<bool>();
            return currentTask.Task;
        }

        public Task<bool> LoginAsync(string password, int? keyNo = null)
        {
            this.password = password;
            return InvokeAsync("Login", "Login", (str) =>
            {
                if (str.Contains("Please input your password."))
                {
                    process.StandardInput.WriteLine(this.password);
                    return null;
                }
                else if (str.Contains("Please choose between"))
                {
                    var findStr = "1 and";
                    var index = str.IndexOf(findStr);
                    var maxNo = Convert.ToInt32(str.Substring(index + findStr.Length, str.Length - index - findStr.Length));

                    if (keyNo >= maxNo || keyNo == null)
                    {
                        process.StandardInput.WriteLine(maxNo);
                    }
                    else {
                        process.StandardInput.WriteLine(keyNo);
                    }
                    return null;
                }
                else if (str.Contains("Login successful !!!"))
                {
                    return true;
                }
                else if (str.Contains("Login failed!"))
                {
                    return false;
                }
                else
                {
                    return null;
                }
            });
        }

        public Task<bool> SendCoinAsync(string address, int amount)
        {
            return InvokeAsync($"SendCoin {address} {amount}", "SendCoin", (str) =>
            {
                if (str.Contains("failed !!"))
                {
                    return false;
                }
                else if (str.Contains("Please confirm that you want to continue enter y or Y, else any other."))
                {
                    process.StandardInput.WriteLine("y");
                    return null;
                }
                else if (str.Contains("Please input your password."))
                {
                    process.StandardInput.WriteLine(this.password);
                    return null;
                }
                else if (str.Contains("successful !!"))
                {
                    return true;
                }
                else
                {
                    return null;
                }
            });
        }

        public Task<bool> TransferAssetAsync(string address, string symbol, int amount)
        {
            return InvokeAsync($"TransferAsset {address} {symbol} {amount}", "TransferAsset", (str) =>
            {
                if (str.Contains("TransferAsset failed!"))
                {
                    return false;
                }
                else if (str.Contains("Please confirm that you want to continue enter y or Y, else any other."))
                {
                    process.StandardInput.WriteLine("y");
                    return null;
                }
                else if (str.Contains("Please input your password."))
                {
                    process.StandardInput.WriteLine(this.password);
                    return null;
                }
                else if (str.Contains("successful !!"))
                {
                    return true;
                }
                else
                {
                    return null;
                }
            });
        }

        public Task<bool> ImportWalletAsync(string pwd, string privateKey)
        {
            return InvokeAsync($"ImportWallet", "ImportWallet", (str) =>
            {
                if (str.Contains("failed"))
                {
                    return false;
                }
                else if (str.Contains("Please input password"))
                {
                    process.StandardInput.WriteLine(pwd);
                    return null;
                }
                else if (str.Contains("Please input private key."))
                {
                    process.StandardInput.WriteLine(privateKey);
                    return null;
                }
                else if (str.Contains("Import a wallet successful"))
                {
                    return true;
                }
                else
                {
                    return null;
                }
            });
        }

        public void Dispose()
        {
            try
            {
                process.Kill();
                process.Dispose();
            }
            catch { }
        }
    }
}
