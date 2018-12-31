using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetSimpleWalletActionRequest
    {
        [FromQuery]
        public string UUID { get; set; }

        [FromQuery]
        public string Sign { get; set; }

        [FromQuery]
        public string TxID { get; set; }

        [FromQuery]
        public int Result { get; set; }

        [FromQuery]
        public string ActionType { get; set; }
    }
}
