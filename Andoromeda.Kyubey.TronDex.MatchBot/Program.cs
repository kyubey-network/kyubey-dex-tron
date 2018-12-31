using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.TronDex.CliNet;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Collections.Generic;

namespace Andoromeda.Kyubey.TronDex.MatchBot
{
    class Program
    {
        static KyubeyContext db;
        static NodeApiInvoker api = new NodeApiInvoker();
        static string dexAddress = ""; // cli绑的哪个账号就填哪个地址
        static TronCliClient tronCliClient = new TronCliClient(@"C:\wallet-cli\", @"build\libs\wallet-cli.jar");
        static string cliWalletPwd = "Passw0rd";

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            var configuration = builder.Build();

            var optionsBuilder = new DbContextOptionsBuilder<KyubeyContext>();
            optionsBuilder.UseMySql(configuration["MySQL"]);
            var dbContext = new KyubeyContext(optionsBuilder.Options);

            Console.WriteLine("Matching bot is starting...");
            await tronCliClient.ImportWalletAsync(cliWalletPwd, "7b81cd82b28dbf9a6efb21de40fb263d83e286644ca04f910f486cb90a7a8357");
            await tronCliClient.LoginAsync(cliWalletPwd);

            while (true)
            {
                var posItem = await db.Constants.SingleOrDefaultAsync(x => x.Id == "exchange_pos");
                var pos = Convert.ToInt64(posItem.Value);
                var result = await api.GetContractTransactionsAsync("TK6EDrMUfiRcso1uR7rNBVDjHRayKPQMoA");
                foreach (var x in result.Data)
                {
                    if (x.Block <= pos)
                    {
                        break;
                    }

                    var tx = await api.GetTransactionAsync(x.TxHash);
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
                        continue;
                    }

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

                    posItem.Value = x.Block.ToString();
                    await db.SaveChangesAsync();
                    await DoMatchAsync(owner, askSymbol, askAmount, bidSymbol, bidAmount);
                }

                await Task.Delay(15000);
            }
        }

        static async Task DoMatchAsync(string account, string askSymbol, long askAmount, string bidSymbol, long bidAmount)
        {
            if (askSymbol == "TRX")
            {
                var price = (double)askAmount / (double)bidAmount;
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

                foreach (var x in orders)
                {
                    var amount = x.Bid < askAmount ? askAmount : x.Bid;
                    x.Bid -= amount; // 2元
                    x.Ask -= amount / x.UnitPrice; // 1个苹果

                    await TransferAsync(x.Account, Convert.ToInt64(amount / x.UnitPrice), bidSymbol);
                    await TransferAsync(account, Convert.ToInt64(amount), askSymbol);

                    db.MatchReceipts.Add(new MatchReceipt
                    {
                        IsSellMatch = true,
                        Asker = account,
                        Ask = amount,
                        Bidder = x.Account,
                        Bid = Convert.ToInt64(amount / x.UnitPrice)
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
            else
            {
                var price = (double)bidAmount / (double)askAmount;
                var order = new DexBuyOrder
                {
                    Account = account,
                    Ask = askAmount,
                    Bid = bidAmount,
                    TokenId = bidSymbol,
                    Time = DateTime.Now,
                    UnitPrice = price
                };

                var orders = await db.DexSellOrders.Where(x => x.TokenId == bidSymbol)
                    .Where(x => x.UnitPrice <= price)
                    .ToListAsync();

                foreach (var x in orders)
                {
                    var amount = x.Bid < askAmount ? askAmount : x.Bid;
                    x.Bid -= amount; // 1个苹果
                    x.Ask -= amount * x.UnitPrice; // 2元

                    await TransferAsync(x.Account, Convert.ToInt64(amount * x.UnitPrice), bidSymbol);
                    await TransferAsync(account, Convert.ToInt64(amount), askSymbol);

                    db.MatchReceipts.Add(new MatchReceipt
                    {
                        IsSellMatch = false,
                        Asker = x.Account,
                        Ask = amount,
                        Bidder = account,
                        Bid = Convert.ToInt64(amount * x.UnitPrice)
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

        static async Task TransferAsync(string address, long amount, string symbol)
        {
            if (symbol == "TRX")
            {
                await tronCliClient.SendCoinAsync(address, amount);
            }
            else if (IsTrc10(symbol))
            {
                await tronCliClient.TransferTRC10Async(address, symbol, amount);
            }
            else if (IsTrc20(symbol))
            {
                await tronCliClient.TransferTRC20Async(address, symbol, amount);
            }
            else
            {
                return;
            }
        }

        static string GetSymbolContract(string symbol)
        {
            var addressDict = new Dictionary<string, string>
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
            if (addressDict.ContainsKey(symbol))
                return addressDict[symbol];
            return null;
        }

        static async Task<string> GetTransferHashAsync(string address, long amount, string symbol)
        {
            if (symbol == "TRX" || IsTrc10(symbol))
            {
                var result = await api.GetTransfersAsync(address, symbol);
                var matched = result.Data.Where(x => x.Amount == amount && x.TransferToAddress == dexAddress);
                foreach (var x in matched)
                {
                    if (!await db.TronTrades.AnyAsync(y => y.TransferHash == x.TransactionHash))
                    {
                        return x.TransactionHash;
                    }
                }

                return null;
            }
            else if (IsTrc20(symbol))
            {
                var result = await api.GetTransactionListAsync(address);
                var matched = result.Data
                    .Where(x => x.SmartCalls
                        .Any(y => y.Calls.FirstOrDefault()?.Name == "transfer"
                            && y.Calls.FirstOrDefault()?.Parameters.First(z => z.Name == "to").Value == dexAddress
                            && Convert.ToInt64(y.Calls.FirstOrDefault()?.Parameters.First(z => z.Name == "value").Value) == amount));
                foreach (var x in matched)
                {
                    if (!await db.TronTrades.AnyAsync(y => y.TransferHash == x.Hash))
                    {
                        return x.Hash;
                    }
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        static bool IsTrc10(string symbol)
        {
            return new[] { "TronGameGlobalPay" }.Contains(symbol);
        }

        static bool IsTrc20(string symbol)
        {
            return new[] { "GOC" }.Contains(symbol);
        }
    }
}
