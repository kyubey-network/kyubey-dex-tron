using Andoromeda.Framework.EosNode;
using System;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetEOSActionsResponse
    {
        public GetActionsResponseActionTraceAct act { get; set; }

        public DateTime time { get; set; }
    }
}
