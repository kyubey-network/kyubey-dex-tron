using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetContentBaseRequest : GetBaseRequest
    {
        [FromRoute(Name = "id")]
        public string Id { get; set; }
    }
}
