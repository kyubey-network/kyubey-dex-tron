using System;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetCurrentOrdersResponse
    {
        public long Id { get; set; }

        public string Type { get; set; }

        public double Price { get; set; }

        public double Amount { get; set; }

        public double Total { get; set; }

        public string Symbol { get; set; }

        public DateTime Time { get; set; }
    }
}
