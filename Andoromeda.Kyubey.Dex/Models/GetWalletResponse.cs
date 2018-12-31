using Newtonsoft.Json;

namespace Andoromeda.Kyubey.Dex.Models
{
    public class GetWalletResponse
    {
        public string IconSrc { get; set; }

        public string Symbol { get; set; }

        public double Valid { get; set; }

        public double Freeze { get; set; }

        public double EOS => (Valid + Freeze) * UnitPrice;

        [JsonIgnore]
        public double UnitPrice { get; set; }
    }
}
