using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetEOSTableRequest : GetPagingRequest
    {
        [FromRoute(Name = "code")]
        public string Code { get; set; }

        [FromRoute(Name = "table")]
        public string Table { get; set; }

        [FromRoute(Name = "account")]
        public string Account { get; set; }
    }
}
