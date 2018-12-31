using System;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetRecentTransactionResponse
    {
        public double UnitPrice { get; set; }

        public double Amount { get; set; }

        public DateTime Time { get; set; }

        public bool Growing { get; set; }
    }
}
