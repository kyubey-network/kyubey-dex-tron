using System;
using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetCandlestickRequest : GetBaseRequest
    {
        [FromQuery(Name = "period")]
        public int Period { get; set; }

        [FromQuery(Name = "begin")]
        public DateTime Begin { get; set; }

        [FromQuery(Name = "end")]
        public DateTime End { get; set; }

        [FromRoute(Name = "id")]
        public string Id { get; set; }
 
    }
}
