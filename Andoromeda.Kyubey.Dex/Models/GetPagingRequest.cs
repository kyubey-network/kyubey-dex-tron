using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetPagingRequest : GetBaseRequest
    {
        [FromQuery(Name = "skip")]
        public int Skip { get; set; }

        [FromQuery(Name = "take")]
        public int Take { get; set; } = 50;
    }
}
