using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Andoromeda.Kyubey.Dex.Models;
using Andoromeda.Kyubey.Dex.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Controllers
{
    [Route("api/v1/lang/{lang}/[controller]")]
    public class NewsController : BaseController
    {
        [HttpGet]
        [ProducesResponseType(typeof(ApiResult<IEnumerable<GetNewsListResponse>>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> List([FromServices] NewsRepositoryFactory newsRepositoryFactory, [FromQuery] GetNewsListRequest request)
        {
            var newsRepository = await newsRepositoryFactory.CreateAsync(request.Lang);
            var responseData = newsRepository
                .EnumerateAll()
                .OrderByDescending(x => x.PublishedAt)
                .Skip(request.Skip)
                .Take(request.Take)
                .Select(x => new GetNewsListResponse()
                {
                    Id = x.Id,
                    Pinned = x.IsPinned,
                    Time = x.PublishedAt,
                    Title = x.Title
                });

            return ApiResult(responseData);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResult<GetNewsContentResponse>), 200)]
        [ProducesResponseType(typeof(ApiResult), 404)]
        public async Task<IActionResult> Content([FromServices] NewsRepositoryFactory newsRepositoryFactory, [FromQuery] GetContentBaseRequest request)
        {
            var newRepository = await newsRepositoryFactory.CreateAsync(request.Lang);
            var newsObj = newRepository.GetSingle(request.Id);

            if (newsObj == null)
            {
                return ApiResult(404, "Not Found");
            }

            return ApiResult(new GetNewsContentResponse()
            {
                Id = newsObj.Id,
                Content = newsObj.Content,
                Pinned = newsObj.IsPinned,
                Time = newsObj.PublishedAt,
                Title = newsObj.Title
            });
        }
    }
}
