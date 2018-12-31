using System;

namespace Andoromeda.Kyubey.Models
{
    public class AlertAccount
    {
        public string Id { get; set; }

        public ulong Position { get; set; }

        public DateTime Begin { get; set; }

        public DateTime Expire { get; set; }

        public ulong SmsSent { get; set; }
    }
}
