using Newtonsoft.Json;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class ContractTransationsResponse
    {
        [JsonProperty("total")]
        public long Total { get; set; }

        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("data")]
        public ContractTransationData[] Data { get; set; }
    }

    public class ContractTransationData
    {
        [JsonProperty("txHash")]
        public string TxHash { get; set; }

        [JsonProperty("parentHash")]
        public string ParentHash { get; set; }

        [JsonProperty("block")]
        public long Block { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("ownAddress")]
        public string OwnAddress { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("ownAddressType")]
        public string OwnAddressType { get; set; }

        [JsonProperty("toAddressType")]
        public string ToAddressType { get; set; }

        [JsonProperty("value")]
        public long Value { get; set; }

        [JsonProperty("txFee")]
        public long TxFee { get; set; }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("call_data")]
        public string CallData { get; set; }
    }

    public class Status
    {
        [JsonProperty("code")]
        public long Code { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
