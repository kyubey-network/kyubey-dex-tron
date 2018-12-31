using Newtonsoft.Json;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class TransferResponse
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("data")]
        public TransferData[] Data { get; set; }
    }

    public class TransferData
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("block")]
        public long Block { get; set; }

        [JsonProperty("transactionHash")]
        public string TransactionHash { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("transferFromAddress")]
        public string TransferFromAddress { get; set; }

        [JsonProperty("transferToAddress")]
        public string TransferToAddress { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("tokenName")]
        public string TokenName { get; set; }

        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
