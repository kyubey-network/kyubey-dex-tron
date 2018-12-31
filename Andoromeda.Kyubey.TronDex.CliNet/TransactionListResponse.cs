using Newtonsoft.Json;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class TransactionListResponse
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("data")]
        public TransationResponse[] Data { get; set; }
    }
}
