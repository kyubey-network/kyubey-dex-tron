using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Andoromeda.Framework.EosNode;
using Andoromeda.Framework.Logger;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Andoromeda.Kyubey.Dex.Repository.TokenRespository;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    [Route("api/v1/lang/{lang}/[controller]")]
    public class UserController : BaseController
    {
        [HttpGet("{account}/favorite")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetFavoriteResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> GetFavorite([FromServices] KyubeyContext db, string account, CancellationToken cancellationToken)
        {
            var last = await db.MatchReceipts
                .OrderByDescending(x => x.Time)
                .GroupBy(x => x.TokenId)
                .Select(x => new
                {
                    TokenId = x.Key,
                    Last = x.FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var last24 = await db.MatchReceipts
                .Where(y => y.Time < DateTime.UtcNow.AddDays(-1))
                .OrderByDescending(x => x.Time)
                .GroupBy(x => x.TokenId)
                .Select(x => new
                {
                    TokenId = x.Key,
                    Last = x.FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            var lastUnitPrice = last.Select(x => new
            {
                id = x.TokenId,
                price = x.Last?.UnitPrice,
                change = (x.Last?.UnitPrice == null
                || last24?.FirstOrDefault(l => l.TokenId == x.TokenId)?.Last?.UnitPrice == null
                || last24?.FirstOrDefault(l => l.TokenId == x.TokenId)?.Last?.UnitPrice == 0)
                ? 0 : ((x.Last?.UnitPrice / last24?.FirstOrDefault(l => l.TokenId == x.TokenId)?.Last?.UnitPrice) - 1)
            });

            var tokendUnitPriceResult = (await db.Tokens.Where(x => x.Status == TokenStatus.Active).ToListAsync(cancellationToken)).Select(x => new
            {
                id = x.Id,
                price = lastUnitPrice.FirstOrDefault(o => o.id == x.Id)?.price ?? 0,
                change = lastUnitPrice.FirstOrDefault(o => o.id == x.Id)?.change ?? 0
            }).ToList();

            var favorite = await db.Favorites
                .Where(x => x.Account == account)
                .ToListAsync(cancellationToken);

            var responseData = new List<GetFavoriteResponse> { };

            responseData.AddRange(tokendUnitPriceResult.Select(
               x => new GetFavoriteResponse
               {
                   Symbol = x.id,
                   UnitPrice = x.price,
                   Change = x.change,
                   Favorite = favorite.Exists(y => y.TokenId == x.id)
               }
            ));

            return ApiResult(responseData);
        }

        [HttpGet("{account}/current-delegate")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetCurrentOrdersResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> GetCurrentDelegate([FromServices] KyubeyContext db, string account, CancellationToken cancellationToken)
        {
            var buy = await db.DexBuyOrders.Where(x => x.Account == account).ToListAsync(cancellationToken);
            var sell = await db.DexSellOrders.Where(x => x.Account == account).ToListAsync(cancellationToken);
            var ret = new List<GetCurrentOrdersResponse>();

            ret.AddRange(buy.Select(x => new GetCurrentOrdersResponse
            {
                Id = x.Id,
                Symbol = x.TokenId,
                Type = "buy",
                Amount = x.Ask,
                Price = x.UnitPrice,
                Total = x.Bid,
                Time = x.Time
            }));

            ret.AddRange(sell.Select(x => new GetCurrentOrdersResponse
            {
                Id = x.Id,
                Symbol = x.TokenId,
                Type = "sell",
                Amount = x.Bid,
                Price = x.UnitPrice,
                Total = x.Ask,
                Time = x.Time
            }));

            return ApiResult(ret.OrderByDescending(x => x.Time));
        }

        [HttpGet("{account}/history-delegate")]
        [ProducesResponseType(typeof(ApiResult<GetPagingResponse<IEnumerable<GetHistoryOrdersResponse>>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> GetHistoryDelegateAsync(GetHistoryDelegateRequest request, [FromServices] KyubeyContext db, CancellationToken cancellationToken)
        {
            var matches = db.MatchReceipts
                .Where(x => x.Bidder == request.Account || x.Asker == request.Account)
                .Select(x => new GetHistoryOrdersResponse
                {
                    Id = x.Id,
                    Symbol = x.TokenId,
                    Bidder = x.IsSellMatch ? x.Bidder : x.Asker,
                    Asker = x.IsSellMatch ? x.Asker : x.Bidder,
                    Type = x.IsSellMatch ? (x.Bidder == request.Account ? "sell" : "buy") : (x.Asker == request.Account ? "sell" : "buy"),
                    UnitPrice = x.UnitPrice,
                    Amount = x.IsSellMatch ? x.Bid : x.Ask,
                    Total = x.IsSellMatch ? x.Ask : x.Bid,
                    Time = x.Time
                }).OrderByDescending(x => x.Time)
            .Where(x => (string.IsNullOrWhiteSpace(request.FilterString) || x.Symbol.Contains(request.FilterString))
                        && (request.Type == null || x.Type == request.Type)
                        && (request.Start == null || request.Start <= x.Time)
                        && (request.End == null || request.End >= x.Time));

            var userHistoryList = matches.Skip(request.Skip).Take(request.Take);
            var rowsCount = await matches.CountAsync(cancellationToken);

            return ApiResult(new GetPagingResponse<GetHistoryOrdersResponse>(userHistoryList, rowsCount, request.Take));
        }

        [HttpGet("{account}/wallet")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetWalletResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> GetWalletAsync(string lang, string account, [FromServices] KyubeyContext db, [FromServices]TokenRepositoryFactory tokenRepositoryFactory, [FromServices]ILogger logger, [FromServices] NodeApiInvoker nodeApiInvoker, CancellationToken cancellationToken)
        {
            var tokenRespository = await tokenRepositoryFactory.CreateAsync(lang);

            var tokensCurrentPrice = await db.MatchReceipts.OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(g => new
            {
                TokenId = g.Key,
                Price = g.FirstOrDefault().UnitPrice
            }).ToListAsync(cancellationToken);

            var buyList = await db.DexBuyOrders
                            .Where(x => x.Account == account)
                            .GroupBy(x => x.TokenId)
                            .Select(x => new
                            {
                                TokenId = x.Key,
                                FreezeEOS = x.Sum(s => s.Bid)
                            }).ToListAsync(cancellationToken);

            var sellList = await db.DexSellOrders
                            .Where(x => x.Account == account)
                            .GroupBy(x => x.TokenId)
                            .Select(x => new
                            {
                                TokenId = x.Key,
                                FreezeToken = x.Sum(s => s.Ask)
                            }).ToListAsync(cancellationToken);

            var matchTokens = await db.MatchReceipts
                .Where(x => x.Time >= DateTime.Now.AddMonths(-3) &&
                (x.Asker == account || x.Bidder == account))
                .Select(x => x.TokenId).Distinct().ToListAsync(cancellationToken);

            var tokens = buyList.Select(x => x.TokenId).Concat(sellList.Select(x => x.TokenId)).Concat(matchTokens).Distinct().ToList();

            var responseData = new List<GetWalletResponse>();

            responseData.Add(new GetWalletResponse()
            {
                IconSrc = "/img/eos.png",
                Freeze = buyList.Sum(x => x.FreezeEOS),
                Symbol = "EOS",
                UnitPrice = 1,
                Valid = nodeApiInvoker.GetCurrencyBalanceAsync(account, "eosio.token", "EOS", cancellationToken).Result
            });

            tokens.ForEach(x =>
                {
                    var currentTokenBalance = 0.0;
                    try
                    {
                        var tokenInfo = tokenRespository.GetSingle(x);
                        if (tokenInfo != null)
                        {
                            currentTokenBalance = nodeApiInvoker.GetCurrencyBalanceAsync(account, tokenInfo?.Basic?.Contract?.Transfer, x, cancellationToken).Result;
                        }
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        logger.LogError(ex.ToString());
                    }
                    finally
                    {
                        responseData.Add(new GetWalletResponse()
                        {
                            IconSrc = $"/token_assets/{x}/icon.png",
                            Valid = currentTokenBalance,
                            Symbol = x,
                            Freeze = sellList.FirstOrDefault(s => s.TokenId == x)?.FreezeToken ?? 0,
                            UnitPrice = tokensCurrentPrice.FirstOrDefault(s => s.TokenId == x)?.Price ?? 0
                        });
                    }
                });

            return ApiResult(responseData);
        }
    }
}
