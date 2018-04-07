using System;

namespace Exchange.Bittrex.Model
{
    public class BittrexGetOrderRecord
    {
        public string AccountId { get; set; }
        public string OrderUuid { get; set; }
        public string Exchange { get; set; } // Pair
        public string Type { get; set; } //"LIMIT_SELL","LIMIT_BUY"
        public decimal? Quantity { get; set; }
        public decimal? QuantityRemaining { get; set; }
        public decimal? Limit { get; set; }
        public decimal? CommissionReserved { get; set; }
        public decimal? CommissionReserveRemaining { get; set; }
        public decimal? CommissionPaid { get; set; }
        public decimal? Price { get; set; }
        public decimal? PricePerUnit { get; set; }
        public DateTime? Opened { get; set; }
        public DateTime? Closed { get; set; }
        public bool? IsOpen { get; set; }
        public string Sentinel { get; set; }
        public bool? CancelInitiated { get; set; }
        public bool? ImmediateOrCancel { get; set; }
        public bool? IsConditional { get; set; }
        public string Condition { get; set; }
        public string ConditionTarget { get; set; }

    }
}
