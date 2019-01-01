using Newtonsoft.Json;
using System;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class ContractResponse
    {
        [JsonProperty("status")]
        public Status Status { get; set; }

        [JsonProperty("data")]
        public ContractModel Data { get; set; }
    }

    public class ContractModel
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("compiler")]
        public string Compiler { get; set; }

        [JsonProperty("isSetting")]
        public bool IsSetting { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("byteCode")]
        public string ByteCode { get; set; }

        [JsonProperty("abi")]
        public string Abi { get; set; }

        [JsonProperty("abiEncoded")]
        public string AbiEncoded { get; set; }

        [JsonProperty("librarys")]
        public object[] Librarys { get; set; }

        [JsonProperty("captchaCode")]
        public string CaptchaCode { get; set; }

        [JsonProperty("creator")]
        public Creator Creator { get; set; }

        [JsonProperty("trc20Name")]
        public string Trc20Name { get; set; }

        [JsonProperty("trc20Symbol")]
        public string Trc20Symbol { get; set; }

        [JsonProperty("trc20Icon")]
        public Uri Trc20Icon { get; set; }
    }

    public class Creator
    {
        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("txHash")]
        public string TxHash { get; set; }

        [JsonProperty("token_balance")]
        public long TokenBalance { get; set; }
    }
}
