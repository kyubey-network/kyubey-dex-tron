using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetBaseRequest
    {
        [FromRoute(Name = "lang")]
        public string Lang { get; set; }
    }
}
