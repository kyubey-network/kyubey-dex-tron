using System;
using System.ComponentModel.DataAnnotations;

namespace Andoromeda.Kyubey.Models
{
    public enum TronTradeStatus
    {
        PendingValidate,
        ValidateFailed,
        ValidateSucceeded
    }

    public class TronTrade
    {
        [MaxLength(64)]
        public string Id { get; set; }

        public TronTradeStatus Status { get; set; }

        [MaxLength(64)]
        public string Account { get; set; }

        public long BidAmount { get; set; }

        [MaxLength(32)]
        public string BidSymbol { get; set; }

        public long AskAmount { get; set; }

        [MaxLength(32)]
        public string AskSymbol { get; set; }

        public DateTime Time { get; set; }

        [MaxLength(64)]
        public string TransferHash { get; set; }
    }
}
