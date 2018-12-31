using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetAbiJsonToBinRequest
    {
        [FromRoute(Name = "code")]
        public string Code { get; set; }

        [FromRoute(Name = "actionName")]
        public string Action { get; set; }

        [FromQuery(Name = "jsonargs")]
        public string JsonArgs { get; set; }
    }
}
