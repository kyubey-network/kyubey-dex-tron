using Microsoft.AspNetCore.Mvc;
using System;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetHistoryDelegateRequest : GetPagingRequest
    {
        [FromRoute(Name = "account")]
        public string Account { get; set; }

        [FromQuery(Name = "filterString")]
        public string FilterString { get; set; }

        [FromQuery(Name = "type")]
        public string Type { get; set; }

        [FromQuery(Name = "start")]
        public DateTime? Start { get; set; }

        [FromQuery(Name = "end")]
        public DateTime? End { get; set; }
    }
}
