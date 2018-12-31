using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetEOSActionsRequest : GetPagingRequest
    {
        [FromRoute(Name = "account")]
        public string Account { get; set; }
    }
}
