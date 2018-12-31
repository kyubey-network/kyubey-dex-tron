namespace Andoromeda.Kyubey.Dex.Models
{
    public class PostSimpleWalletLoginRequest
    {
        public string Protocol { get; set; }

        public string Version { get; set; }

        public long Timestamp { get; set; }

        public string Sign { get; set; }

        public string UUID { get; set; }

        public string Account { get; set; }

        public string Ref { get; set; }
    }
}
