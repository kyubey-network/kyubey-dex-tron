using Microsoft.AspNetCore.Mvc;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetSimpleWalletExchangeRequest
    {
        [FromQuery]
        public string UUID { get; set; }

        [FromQuery]
        public string Sign { get; set; }

        [FromQuery]
        public string TxID { get; set; }

        [FromQuery]
        public string Result { get; set; }
    }
}
