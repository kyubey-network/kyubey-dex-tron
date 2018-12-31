using Newtonsoft.Json;

namespace Andoromeda.Kyubey.TronDex.CliNet
{
    public class TransationResponse
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("block")]
        public long Block { get; set; }

        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("timestamp")]
        public long Timestamp { get; set; }

        [JsonProperty("ownerAddress")]
        public string OwnerAddress { get; set; }

        [JsonProperty("toAddress")]
        public string ToAddress { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("contractType")]
        public long ContractType { get; set; }

        [JsonProperty("confirmed")]
        public bool Confirmed { get; set; }

        [JsonProperty("contractData")]
        public ContractData ContractData { get; set; }

        [JsonProperty("fee")]
        public string Fee { get; set; }

        [JsonProperty("SmartCalls")]
        public SmartCall[] SmartCalls { get; set; }

        [JsonProperty("Events")]
        public object Events { get; set; }
    }

    public partial class ContractData
    {
        [JsonProperty("contract_address")]
        public string ContractAddress { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }

        [JsonProperty("owner_address")]
        public string OwnerAddress { get; set; }
    }

    public partial class SmartCall
    {
        [JsonProperty("Owner")]
        public string Owner { get; set; }

        [JsonProperty("Contract")]
        public string Contract { get; set; }

        [JsonProperty("CallValue")]
        public long CallValue { get; set; }

        [JsonProperty("TokenID")]
        public long TokenId { get; set; }

        [JsonProperty("CallTokenValue")]
        public long CallTokenValue { get; set; }

        [JsonProperty("Calls")]
        public Call[] Calls { get; set; }
    }

    public partial class Call
    {
        [JsonProperty("Contract")]
        public string Contract { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Sign")]
        public string Sign { get; set; }

        [JsonProperty("Type")]
        public long Type { get; set; }

        [JsonProperty("Constant")]
        public bool Constant { get; set; }

        [JsonProperty("Payable")]
        public bool Payable { get; set; }

        [JsonProperty("StateMutability")]
        public long StateMutability { get; set; }

        [JsonProperty("Parameters")]
        public Parameter[] Parameters { get; set; }
    }

    public partial class Parameter
    {
        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Type")]
        public string Type { get; set; }

        [JsonProperty("Value")]
        public string Value { get; set; }
    }
}
