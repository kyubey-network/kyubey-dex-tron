using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Andoromeda.Kyubey.Models;
using Andoromeda.Kyubey.TronDex.CliNet;

namespace Andoromeda.Kyubey.TronDex.MatchBot
{
    class Program
    {
        static KyubeyContext db;
        static NodeApiInvoker api = new NodeApiInvoker();
        static string dexAddress = ""; // cli绑的哪个账号就填哪个地址

        static async Task Main(string[] args)
        {
            // TODO: 初始化一下EF

            Console.WriteLine("Matching bot is starting...");
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
            // TODO: transfer TRX, TRC10, TRC20 to address
            if (symbol == "TRX")
            {

            }
            else if (IsTrc10(symbol))
            {

            }
            else if (IsTrc20(symbol))
            {

            }
            else
            {
                return;
            }
        }

        static string GetSymbolContract(string symbol)
        {
            // TODO: Return symbol's distributing contract address
            return "";
        }

        static async Task<string> GetTransferHashAsync(string address, long amount, string symbol)
        {
            if (symbol == "TRX" || IsTrc10(symbol))
            {
                var result = await api.GetTransfersAsync(address, symbol);
                var matched = result.Data.Where(x => x.Amount == amount && x.TransferToAddress == dexAddress);
                foreach(var x in matched)
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
                foreach(var x in matched)
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
