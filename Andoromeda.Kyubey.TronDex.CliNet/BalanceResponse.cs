using Newtonsoft.Json;
using System.Collections.Generic;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class BalanceResponse
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("data")]
        public BalanceData[] Data { get; set; }
    }

    public partial class BalanceData
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("balance")]
        public long Balance { get; set; }

        [JsonProperty("power")]
        public long Power { get; set; }

        [JsonProperty("tokenBalances")]
        public Dictionary<string, double> TokenBalances { get; set; }

        [JsonProperty("dateUpdated")]
        public long DateUpdated { get; set; }

        [JsonProperty("dateCreated")]
        public long DateCreated { get; set; }

        [JsonProperty("accountType")]
        public string AccountType { get; set; }
    }
}
