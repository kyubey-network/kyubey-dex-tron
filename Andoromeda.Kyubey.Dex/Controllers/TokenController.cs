using Andoromeda.Framework.EosNode;
using Andoromeda.Framework.Logger;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Andoromeda.Kyubey.Dex.Repository.TokenRespository;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    [Route("api/v1/lang/{lang}/[controller]")]
    public class TokenController : BaseController
    {
        [HttpGet]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetTokenListResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> TokenList([FromServices] KyubeyContext db, CancellationToken cancellationToken)
        {
            var todayList = await db.MatchReceipts.Where(x => x.Time >= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice,
                MaxPrice = x.Max(c => c.UnitPrice),
                MinPrice = x.Min(c => c.UnitPrice),
                Volume = x.Sum(c => c.IsSellMatch ? c.Bid : c.Ask)
            }).ToListAsync(cancellationToken);

            var lastList = await db.MatchReceipts.Where(x => x.Time <= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice
            }).ToListAsync(cancellationToken);

            var responseData = (await db.Tokens.OrderByDescending(x => x.Priority).ToListAsync(cancellationToken)).Select(x => new GetTokenListResponse()
            {
                icon_src = $"/token_assets/{x.Id}/icon.png",
                current_price = todayList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice ?? lastList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice ?? 0,
                change_recent_day =
                    (todayList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice == null ||
                    lastList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice == null ||
                    lastList.FirstOrDefault(t => t.TokenId == x.Id)?.CurrentPrice == 0) ?
                    0 : (todayList.FirstOrDefault(t => t.TokenId == x.Id).CurrentPrice / lastList.FirstOrDefault(t => t.TokenId == x.Id).CurrentPrice) - 1,
                is_recommend = true,
                max_price_recent_day = todayList.FirstOrDefault(s => s.TokenId == x.Id)?.MaxPrice ?? 0,
                min_price_recent_day = todayList.FirstOrDefault(s => s.TokenId == x.Id)?.MinPrice ?? 0,
                symbol = x.Id,
                volume_recent_day = todayList.FirstOrDefault(s => s.TokenId == x.Id)?.Volume ?? 0,
                priority = x.Priority,
                newdex_price_ask = x.NewDexAsk,
                newdex_price_bid = x.NewDexBid,
                whaleex_price = x.WhaleExPrice
            });

            return ApiResult(responseData);
        }

        [HttpGet("{symbol}/buy-order")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetBaseOrderResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> BuyOrder([FromServices] KyubeyContext db, string symbol, CancellationToken cancellationToken)
        {
            var orders = await db.DexBuyOrders
                        .Where(x => x.TokenId == symbol)
                        .OrderByDescending(x => x.UnitPrice)
                        .Take(15)
                        .ToListAsync(cancellationToken);

            var responseData = orders
                .GroupBy(x => x.UnitPrice)
                .Select(x => new GetBaseOrderResponse
                {
                    UnitPrice = x.Key,
                    Amount = x.Select(y => y.Ask).Sum(),
                    Total = x.Select(y => y.Bid).Sum()
                });

            return ApiResult(responseData, new { symbol = symbol });
        }

        [HttpGet("{symbol}/sell-order")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetBaseOrderResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> SellOrder([FromServices] KyubeyContext db, string symbol, CancellationToken cancellationToken)
        {
            var orders = await db.DexSellOrders
                        .Where(x => x.TokenId == symbol)
                        .OrderBy(x => x.UnitPrice)
                        .Take(15)
                        .ToListAsync(cancellationToken);
            orders.Reverse();

            var responseData = orders
                .GroupBy(x => x.UnitPrice)
                .Select(x => new GetBaseOrderResponse
                {
                    UnitPrice = x.Key,
                    Amount = x.Select(y => y.Bid).Sum(),
                    Total = x.Select(y => y.Ask).Sum()
                });

            return ApiResult(responseData, new { symbol = symbol });
        }

        [HttpGet("{symbol}/match")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetRecentTransactionResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> RecentTransactionRecord([FromServices] KyubeyContext db, string symbol, CancellationToken token)
        {
            var responseData = await db.MatchReceipts
                .Where(x => x.TokenId == symbol)
                .OrderByDescending(x => x.Time)
                .Take(21)
                .ToListAsync(token);

            return ApiResult(responseData.Take(20).Select(x => new GetRecentTransactionResponse
            {
                UnitPrice = x.UnitPrice,
                Amount = (x.IsSellMatch ? x.Bid : x.Ask),
                Time = x.Time,
                Growing = x.UnitPrice > (responseData.FirstOrDefault(g => g.Time < x.Time)?.UnitPrice ?? x.UnitPrice)
            }), new { symbol = symbol });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResult<GetTokenDetailResponse>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> TokenDetails(
            string id,
            GetBaseRequest request,
            [FromServices] KyubeyContext db,
            [FromServices] TokenRepositoryFactory tokenRepositoryFactory,
            [FromServices] NodeApiInvoker nodeApiInvoker,
            [FromServices]ILogger logger,
            CancellationToken cancellationToken
            )
        {
            var todayItem = await db.MatchReceipts.Where(x => x.TokenId == id && x.Time >= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice,
                MaxPrice = x.Max(c => c.UnitPrice),
                MinPrice = x.Min(c => c.UnitPrice),
                Volume = x.Sum(c => c.IsSellMatch ? c.Bid : c.Ask)
            }).FirstOrDefaultAsync(cancellationToken);

            var lastItem = await db.MatchReceipts.Where(x => x.TokenId == id && x.Time <= DateTime.Now.AddDays(-1)).OrderByDescending(x => x.Time).GroupBy(x => x.TokenId).Select(x => new
            {
                TokenId = x.Key,
                CurrentPrice = x.FirstOrDefault().UnitPrice
            }).FirstOrDefaultAsync(cancellationToken);

            var tokenRepository = await tokenRepositoryFactory.CreateAsync(request.Lang);
            var token = tokenRepository.GetSingle(id);

            GetSymbolSupplyResponse symbolSupply = null;
            try
            {
                symbolSupply = await nodeApiInvoker.GetSymbolSupplyAsync(token?.Basic?.Contract?.Transfer, id, cancellationToken);
            }
            catch (ArgumentNullException ex)
            {
                logger.LogError(ex.ToString());
            }

            var responseData = new GetTokenDetailResponse()
            {
                Symbol = token.Id,
                ChangeRecentDay = (todayItem?.CurrentPrice == null || ((lastItem?.CurrentPrice ?? 0) == 0)) ? 0 :
((todayItem?.CurrentPrice ?? 0) / (lastItem?.CurrentPrice ?? 1) - 1),
                CurrentPrice = todayItem?.CurrentPrice ?? lastItem?.CurrentPrice ?? 0,
                MaxPriceRecentDay = todayItem?.MaxPrice ?? 0,
                MinPriceRecentDay = todayItem?.MinPrice ?? 0,
                VolumeRecentDay = todayItem?.Volume ?? 0,
                IsRecommend = true,
                IconSrc = $"/token_assets/{token.Id}/icon.png",
                Priority = token.Priority,
                Description = tokenRepository.GetTokenDescription(id, request.Lang),
                TotalSupply = symbolSupply?.MaxSupply ?? 0,
                TotalCirculate = symbolSupply?.Supply ?? 0,
                Contract = new GetTokenResultContract()
                {
                    Depot = token.Basic?.Contract?.Depot,
                    Pricing = token.Basic?.Contract?.Pricing,
                    Transfer = token.Basic?.Contract?.Transfer
                },
                Website = token.Basic.Website
            };
            return ApiResult(responseData);
        }


        [HttpGet("{id}/candlestick")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetCandlestickResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> Candlestick([FromServices] KyubeyContext db, GetCandlestickRequest request, CancellationToken token)
        {
            var ticks = new TimeSpan(0, 0, request.Period);
            var begin = new DateTime(request.Begin.Ticks / ticks.Ticks * ticks.Ticks).ToUniversalTime();
            var end = new DateTime(request.End.Ticks / ticks.Ticks * ticks.Ticks).ToUniversalTime();

            var data = await db.MatchReceipts
                 .Where(x => x.TokenId == request.Id)
                 .Where(x => x.Time < end)
                 .OrderBy(x => x.Time)
                 .GroupBy(x => x.Time >= begin ? x.Time.Ticks / ticks.Ticks * ticks.Ticks : 0)
                 .Select(x => new GetCandlestickResponse
                 {
                     Time = new DateTime(x.Key),
                     Min = x.Select(y => y.UnitPrice).Min(),
                     Max = x.Select(y => y.UnitPrice).Max(),
                     Opening = x.Select(y => y.UnitPrice).FirstOrDefault(),
                     Closing = x.OrderByDescending(y => y.Time).Select(y => y.UnitPrice).FirstOrDefault(),
                     Volume = x.Count()
                 })
                 .ToListAsync(token);

            if (data.Count <= 1) return ApiResult(data.Where(x => x.Time >= begin).OrderBy(x => x.Time));

            //repair data by ticks
            for (var i = begin; i < end; i = i.Add(ticks))
            {
                if (data.Any(x => x.Time == i)) continue;
                var prev = data
                    .Where(x => x.Time < i)
                    .OrderBy(x => x.Time)
                    .LastOrDefault();
                if (prev == null) continue;
                data.Add(new GetCandlestickResponse
                {
                    Min = prev.Closing,
                    Max = prev.Closing,
                    Closing = prev.Closing,
                    Opening = prev.Closing,
                    Time = i,
                    Volume = 0
                });
            }

            var responseData = data.Where(x => x.Time >= begin).OrderBy(x => x.Time).ToList();
            responseData.ForEach(x => x.Time = x.Time.ToLocalTime());

            return ApiResult(responseData);
        }

    }
}
