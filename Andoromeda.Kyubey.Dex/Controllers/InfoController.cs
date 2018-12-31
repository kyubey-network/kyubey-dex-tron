using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Dex.Repository;
using Microsoft.AspNetCore.Mvc;
using Andoromeda.Kyubey.Models;
using System;
using Microsoft.EntityFrameworkCore;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    [Route("api/v1/lang/{lang}/[controller]")]
    public class InfoController : BaseController
    {
        [HttpGet("slides")]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetSlidesResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> Banner([FromServices] SlidesRepositoryFactory slidesRepositoryFactory, [FromQuery]GetSlidesRequest request)
        {
            var newSlides = await slidesRepositoryFactory.CreateAsync(request.Lang);
            var responseData = newSlides.EnumerateAll()
                .Select(x => new GetSlidesResponse()
                {
                    Background = $"/slides_assets/{x.Background}".Replace(@"\", "/"),
                    Foreground = $"/slides_assets/{x.Foreground}".Replace(@"\", "/")
                });
            return ApiResult(responseData);
        }

        [HttpGet("volume")]
        [ProducesResponseType(typeof(ApiResult<double>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> Volume([FromServices] KyubeyContext db)
        {
            var volumeVal = await db.MatchReceipts.Where(x => x.Time > DateTime.Now.AddDays(-1)).SumAsync(x => x.IsSellMatch ? x.Ask : x.Bid);
            return ApiResult(volumeVal);
        }
    }
}
