using System;
using System.Collections.Generic;
using Common.Contracts;
using Newtonsoft.Json;

namespace Exchange.Kraken.Model
{
    public class KrakenOrder
    {
        [JsonProperty(PropertyName = "TxId")]
        public string TxId { get; set; }

        //Asset pair
        public string Pair { get; set; }

        [JsonProperty(PropertyName = "status")]
        public KrakenOrderStatus Status { get; set; }

        //Type of order (buy or sell)
        public string Type { get; set; }
        //Execution type
        public string OrderType { get; set; }
        //Price. Optional. Dependent upon order type
        public decimal? Price { get; set; }

        //Secondary price. Optional. Dependent upon order type
        public decimal? Price2 { get; set; }
        //Order volume in lots
        [JsonProperty(PropertyName = "vol")]
        public decimal Volume { get; set; }

        //Amount of leverage required. Optional. default none
        public string Leverage { get; set; }
        //Position tx id to close (optional.  used to close positions)
        public string Position { get; set; }
        //list of order flags (optional):
        public string OFlags { get; set; }
        //Scheduled start time. Optional
        public DateTime? Opened { get; set; }
        public string Starttm { get; set; }
        //Expiration time. Optional
        public string Expiretm { get; set; }
        //User ref id. Optional
        public string Userref { get; set; }
        //Validate inputs only. do not submit order. Optional
        public bool Validate { get; set; }
        //Closing order details
        public Dictionary<string, string> Close { get; set; }

        public KrakenOrder(KrakenGetOrderRecord record)
        {
            Pair = record.Descr.Pair;
            Type = record.Descr.Type;
            OrderType = record.Descr.Ordertype;
            Price = Convert.ToDecimal(record.Descr.Price);
            Volume = Convert.ToDecimal(record.Vol);
            Opened = UnixTime.ConvertToDateTime((UInt32)(record.Opentm));
            Status = record.Status;
        }

        public KrakenOrder()
        {            
        }
    }
    
    public class KrakenGetOrderRecord
    {
        [JsonProperty(PropertyName = "refid")]
        public object Refid { get; set; }
        [JsonProperty(PropertyName = "userref")]
        public object Userref { get; set; }
        [JsonProperty(PropertyName = "status")]
        public KrakenOrderStatus Status { get; set; }
        [JsonProperty(PropertyName = "opentm")]
        public double Opentm { get; set; }
        [JsonProperty(PropertyName = "starttm")]
        public string Starttm { get; set; }
        [JsonProperty(PropertyName = "expiretm")]
        public string Expiretm { get; set; }
        [JsonProperty(PropertyName = "descr")]
        public Descr Descr { get; set; }
        [JsonProperty(PropertyName = "vol")]
        public decimal Vol { get; set; }
        [JsonProperty(PropertyName = "vol_exec")]
        public decimal VolExec { get; set; }
        [JsonProperty(PropertyName = "cost")]
        public string Cost { get; set; }
        [JsonProperty(PropertyName = "fee")]
        public decimal Fee { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "misc")]
        public string Misc { get; set; }
        [JsonProperty(PropertyName = "oflags")]
        public string Oflags { get; set; }
    }

    public class Descr
    {
        [JsonProperty(PropertyName = "pair")]
        public string Pair { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "ordertype")]
        public string Ordertype { get; set; }
        [JsonProperty(PropertyName = "price")]
        public string Price { get; set; }
        [JsonProperty(PropertyName = "price2")]
        public string Price2 { get; set; }
        [JsonProperty(PropertyName = "leverage")]
        public string Leverage { get; set; }
        [JsonProperty(PropertyName = "order")]
        public string Order { get; set; }
    }

    public enum TradeType
    {
        Sell,
        Buy
    }

    public enum KrakenOrderType
    {
        Market = 1,
        Limit = 2,// (price = limit price)
        StopLoss = 3, // (price = stop loss price)
        TakeProfit = 4, // (price = take profit price)
        StopLossProfit = 5, // (price = stop loss price, price2 = take profit price)
        StopLossProfitLimit = 6, // (price = stop loss price, price2 = take profit price)
        StopLossLimit = 7,// (price = stop loss trigger price, price2 = triggered limit price)
        TakeProfitLimit = 8, // (price = take profit trigger price, price2 = triggered limit price)
        TrailingStop = 9, //(price = trailing stop offset)
        TrailingStopLimit = 10,// (price = trailing stop offset, price2 = triggered limit offset)
        StopLossAndLimit = 11,// (price = stop loss price, price2 = limit price)
    }

    public enum KrakenOrderStatus
    {
        Pending = 1, // order pending book entry
        Open = 2, // open order
        Closed = 3, //closed order
        Canceled = 4, // order canceled
        Expired = 5 // order expired
    }

    public enum OFlag
    {
        Viqc = 1, //volume in quote currency
        Plbc = 2, //prefer profit/los in base currency
        Nompp = 3 //no market price protection
    }
}
