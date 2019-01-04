using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.TronDex.CliNet;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Andoromeda.Kyubey.TronDex.MatchBot
{
    class Program
    {
        static KyubeyContext db;
        static NodeApiInvoker api = new NodeApiInvoker();
        static string dexTransferAddress = "TBVbLiQirADEdMsTL4WeTgNmMAgeoS16cF"; // cli绑的哪个账号就填哪个地址
        const string dexContractAddress = "TK6EDrMUfiRcso1uR7rNBVDjHRayKPQMoA";
        static TronCliClient tronCliClient;
        static string cliWalletPwd = "Passw0rd";

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<KyubeyContext>();
            optionsBuilder.UseMySql(configuration["MySQL"]);
            db = new KyubeyContext(optionsBuilder.Options);

            tronCliClient = new TronCliClient(configuration["WalletWorkPath"], configuration["WalletFilePath"]);

            Console.WriteLine("Matching bot is starting...");
            //await tronCliClient.ImportWalletAsync(cliWalletPwd, "7b81cd82b28dbf9a6efb21de40fb263d83e286644ca04f910f486cb90a7a8357");
            await tronCliClient.LoginAsync(cliWalletPwd);

            while (true)
            {
                var posItem = await db.Constants.FirstOrDefaultAsync(x => x.Id == "exchange_pos");
                var pos = Convert.ToInt64(posItem.Value);
                var result = await api.GetContractTransactionsAsync(dexContractAddress);
                foreach (var x in result.Data.OrderBy(x => x.Block))
                {
                    if (x.Block <= pos)
                    {
                        continue;
                    }

                    var tx = await api.GetTransactionAsync(x.TxHash);
                    if (tx.SmartCalls == null)
                    {
                        Thread.Sleep(1000);
                        tx = await api.GetTransactionAsync(x.TxHash);
                    }
                    if (tx.SmartCalls == null)
                    {
                        Thread.Sleep(3000);
                        tx = await api.GetTransactionAsync(x.TxHash);
                    }
                    if (tx.SmartCalls == null)
                    {
                        Thread.Sleep(5000);
                        tx = await api.GetTransactionAsync(x.TxHash);
                    }

                    var owner = tx.SmartCalls.First().Owner;
                    var call = tx.SmartCalls.First().Calls.FirstOrDefault(y => y.Name == "exchange");
                    if (call == null)
                    {
                        continue;
                    }
                    var bidSymbol = call.Parameters.First(y => y.Name == "bid").Value;
                    var bidAmount = Convert.ToInt64(call.Parameters.First(y => y.Name == "bidamount").Value);
                    var askSymbol = call.Parameters.First(y => y.Name == "ask").Value;
                    var askAmount = Convert.ToInt64(call.Parameters.First(y => y.Name == "askamount").Value);
                    var transferHash = await GetTransferHashAsync(owner, bidAmount, bidSymbol);

                    if (transferHash == null)
                    {
                        if (!db.TronTrades.Any(t => t.Id == x.TxHash))
                        {
                            db.TronTrades.Add(new TronTrade
                            {
                                Id = x.TxHash,
                                Status = TronTradeStatus.ValidateFailed,
                                Account = owner,
                                AskAmount = askAmount,
                                AskSymbol = askSymbol,
                                BidAmount = bidAmount,
                                BidSymbol = bidSymbol,
                                Time = new DateTime(x.Timestamp),
                                TransferHash = null
                            });
                            await db.SaveChangesAsync();
                        }
                        continue;
                    }

                    var dbInstance = db.TronTrades.FirstOrDefault(t => t.Id == x.TxHash);
                    if (dbInstance == null)
                    {
                        db.TronTrades.Add(new TronTrade
                        {
                            Id = x.TxHash,
                            Status = TronTradeStatus.ValidateFailed,
                            Account = owner,
                            AskAmount = askAmount,
                            AskSymbol = askSymbol,
                            BidAmount = bidAmount,
                            BidSymbol = bidSymbol,
                            Time = new DateTime(x.Timestamp),
                            TransferHash = transferHash
                        });
                    }
                    else
                    {
                        dbInstance.TransferHash = transferHash;
                    }

                    posItem.Value = x.Block.ToString();
                    await db.SaveChangesAsync();
                    await DoMatchAsync(transferHash, owner, askSymbol, askAmount, bidSymbol, bidAmount);
                }

                await Task.Delay(2000);
            }
        }

        static async Task DoMatchAsync(string transferHash, string account, string askSymbol, double askAmount, string bidSymbol, double bidAmount)
        {
            askAmount /= 1000000;
            bidAmount /= 1000000;

            //sell
            if (askSymbol == "TRX")
            {
                var price = askAmount / bidAmount;
                var order = new DexSellOrder
                {
                    Account = account,
                    Ask = askAmount,
                    Bid = bidAmount,
                    TokenId = bidSymbol,
                    Time = DateTime.Now,
                    UnitPrice = price
                };

                var orders = await db.DexBuyOrders.Where(x => x.TokenId == bidSymbol)
                    .Where(x => x.UnitPrice >= price)
                    .ToListAsync();

                var remianAmount = bidAmount;

                foreach (var x in orders)
                {
                    if (remianAmount <= 0)
                        break;
                    //token amount
                    var amount = x.Ask < remianAmount ? x.Ask : remianAmount;
                    x.Ask -= amount; // 2元
                    x.Bid -= amount * x.UnitPrice; // 1个苹果

                    order.Bid -= amount;
                    order.Ask -= amount * price;

                    remianAmount -= amount;

                    await TransferAsync(x.Account, amount, bidSymbol);
                    await TransferAsync(account, amount * price, askSymbol);

                    db.MatchReceipts.Add(new MatchReceipt
                    {
                        IsSellMatch = true,
                        Asker = account,
                        Ask = amount * x.UnitPrice,
                        Bidder = x.Account,
                        Bid = amount,
                        Time = DateTime.Now,
                        TokenId = bidSymbol,
                        UnitPrice = x.UnitPrice
                    });

                    if (x.Ask == 0 || x.Bid == 0)
                    {
                        db.Remove(x);
                    }
                }

                if (order.Ask != 0 && order.Bid != 0)
                {
                    db.DexSellOrders.Add(order);
                }

                await db.SaveChangesAsync();
            }
            //buy
            else
            {
                double price = bidAmount / askAmount;
                var order = new DexBuyOrder
                {
                    TransferHash = transferHash,
                    Account = account,
                    Ask = askAmount,
                    Bid = bidAmount,
                    TokenId = askSymbol,
                    Time = DateTime.Now,
                    UnitPrice = price
                };

                var orders = await db.DexSellOrders.Where(x => x.TokenId == askSymbol)
                    .Where(x => x.UnitPrice <= price)
                    .ToListAsync();

                var remianAmount = askAmount;

                foreach (var x in orders)
                {
                    if (remianAmount <= 0)
                        break;

                    var amount = x.Bid < remianAmount ? x.Bid : remianAmount;
                    x.Bid -= amount; // 1个苹果
                    x.Ask -= amount * x.UnitPrice; // 2元

                    order.Ask -= amount;
                    order.Bid -= amount * x.UnitPrice;

                    remianAmount -= amount;

                    await TransferAsync(x.Account, amount * x.UnitPrice, bidSymbol);
                    await TransferAsync(account, amount, askSymbol);

                    db.MatchReceipts.Add(new MatchReceipt
                    {
                        IsSellMatch = false,
                        Asker = account,
                        Ask = amount,
                        Bidder = x.Account,
                        Bid = amount * x.UnitPrice,
                        UnitPrice = price,
                        TokenId = askSymbol,
                        Time = DateTime.Now
                    });

                    if (x.Ask == 0 || x.Bid == 0)
                    {
                        db.Remove(x);
                    }
                }

                if (order.Ask != 0 && order.Bid != 0)
                {
                    db.DexBuyOrders.Add(order);
                }

                await db.SaveChangesAsync();
            }
        }

        static async Task TransferAsync(string address, double amount, string symbol)
        {
            if (Convert.ToInt64(amount) <= 0)
                return;

            if (symbol == "TRX")
            {
                await tronCliClient.SendCoinAsync(address, Convert.ToInt64(1000000 * amount));
            }
            else if (IsTrc10(symbol))
            {
                await tronCliClient.TransferTRC10Async(address, symbol, Convert.ToInt64(amount));
            }
            else if (IsTrc20(symbol))
            {
                await tronCliClient.TransferTRC20Async(address, symbolAddress[symbol], Convert.ToInt64(1000000 * amount));
            }
            else
            {
                return;
            }
        }

        private static Dictionary<string, string> symbolAddress = new Dictionary<string, string>
        {
            {"DEX","TF6i3aPkvhQ7Whqa8UDs7VXVhtURasnAMk" },
            {"PCB","TJzcEaqgYk9g4jZLEsB2DksLGikM4dWwYJ" },
            {"DRS","TKBURAzYP6hwcRWBzqZvqww2PZuBm5Lev7" },
            {"RET","TLCiRv2qn9tP3x59B3jtxuonyQzUHwNyUq" },
            {"DICE","THvZvKPLHKLJhEFYKiyqj6j8G8nGgfg7ur" },
            {"BET","TWGZ7HnAhZkvxiT89vCBSd6Pzwin5vt3ZA" },
            {"GOC","TYe6uNj7jxkwy28yXeLPs6KDLZCuUjXvgd" },
            {"AB","TNbYoP22d74RWy4ETssHsXYFrnmmbQ2fvt" },
            {"BFC","TYUbxiksCwDyAfNcmirnCATZgb6hyrGbir" },
            {"WIN","TBAo7PNyKo94YWUq1Cs2LBFxkhTphnAE4T" },
            {"GAME","TYPHiHUiPBPCNvqBpzy1f7bdqrZ5r8e1K7" },
            {"TWJ","TNq5PbSssK5XfmSYU4Aox4XkgTdpDoEDiY" },
            {"ANTE","TCN77KWWyUyi2A4Cu7vrh5dnmRyvUuME1E" },
            {"6KPEN","TCMjU3taxp19xNWMFQdQw45CYwQcqrsYqA" },
            {"CFT","TSkG9SSKdWV5QBuTPN6udi48rym5iPpLof" },
            {"VCOIN","TNisVGhbxrJiEHyYUMPxRzgytUtGM7vssZ" },
            {"REY","TMWkPhsb1dnkAVNy8ej53KrFNGWy9BJrfu" },
            {"PLAY","TYbSzw3PqBWohc4DdyzFDJMd1hWeNN6FkB" },
            {"RING","TL175uyihLqQD656aFx3uhHYe1tyGkmXaW" }
        };

        static string GetSymbolContract(string symbol)
        {
            if (symbolAddress.ContainsKey(symbol))
                return symbolAddress[symbol];
            return null;
        }

        static async Task<string> GetTransferHashAsync(string userAddress, long amount, string symbol)
        {
            if (symbol == "TRX" || IsTrc10(symbol))
            {
                var result = await api.GetTransfersAsync(userAddress, symbol);
                var matched = result.Data.Where(x => x.Amount == amount && x.TransferToAddress == dexTransferAddress);
                foreach (var x in matched)
                {
                    if (!await db.TronTrades.AnyAsync(y => y.TransferHash == x.TransactionHash))
                    {
                        return x.TransactionHash;
                    }
                }
            }
            else if (IsTrc20(symbol))
            {
                var result = await api.GetTransactionByAddressAsync(userAddress, take: 1000);
                var userTransDbLogs = await db.TronTrades.Where(x => x.Account == userAddress).ToListAsync();
                var userTransChainLogs = result.Data.Where(x => x.ToAddress == symbolAddress[symbol]).ToList();

                foreach (var ucLog in result.Data.Where(x => x.ToAddress == symbolAddress[symbol]))
                {
                    if (userTransDbLogs.Any(x => x.TransferHash == ucLog.Hash))
                        continue;

                    var logDetail = await api.GetTransactionAsync(ucLog.Hash);

                    if (logDetail.SmartCalls != null)
                    {
                        if (symbolAddress[symbol] != logDetail.SmartCalls.FirstOrDefault().Contract)
                            continue;

                        var callObj = logDetail.SmartCalls.FirstOrDefault()?.Calls.FirstOrDefault(x => x.Name == "transfer");
                        if (callObj != null)
                        {
                            if (callObj.Parameters.Any(x => x.Name == "value" && Convert.ToInt64(x.Value) == amount)
                                && callObj.Parameters.Any(x => x.Name == "to" && x.Value == dexTransferAddress))
                            {
                                return ucLog.Hash;
                            }
                        }
                    }
                }
            }
            return null;
        }

        static bool IsTrc10(string symbol)
        {
            return !IsTrc20(symbol);
        }

        static bool IsTrc20(string symbol)
        {
            return symbolAddress.ContainsKey(symbol);
        }
    }
}
