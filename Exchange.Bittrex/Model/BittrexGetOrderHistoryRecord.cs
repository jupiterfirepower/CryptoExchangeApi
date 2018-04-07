using System;

namespace Exchange.Bittrex.Model
{
    public class BittrexGetOrderHistoryRecord
    {
        public string OrderUuid { get; set; }
        public string Exchange { get; set; } // Pair
        public DateTime TimeStamp { get; set; }
        public string OrderType { get; set; } //"LIMIT_SELL","LIMIT_BUY"
        public decimal Limit { get; set; }
        public decimal Quantity { get; set; }
        public decimal QuantityRemaining { get; set; }
        public decimal Commission { get; set; }
        public decimal Price { get; set; }
        public decimal PricePerUnit { get; set; }
        public bool IsConditional { get; set; }
        public string Condition { get; set; }
        public string ConditionTarget { get; set; }
        public bool ImmediateOrCancel { get; set; }

    }
}
