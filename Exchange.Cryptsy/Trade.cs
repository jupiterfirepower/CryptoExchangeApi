/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Exchange.Cryptsy
{
    public class Trade
    {
        public Int64 TradeId { get; private set; }
        public DateTime DateTimeUtc { get; private set; }
        public decimal UnitPrice { get; private set; }
        public decimal Quantity { get; private set; }
        public decimal TotalPrice { get; private set; }
        public CryptsyOrder.ORDER_TYPE InitiateOrderType { get; private set; }
        public CryptsyOrder.ORDER_TYPE TradeType { get; private set; }
        public Int64 OrderId { get; private set; } //Original order id this trade was executed against. -1 if not applicable
        public decimal Fee { get; private set; }

        public static Trade ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;

            /*
             * Method: mytrades (authenticated)
             * Present properties: tradeid, tradetype, datetime, tradeprice, quantity, total, fee, initiate_ordertype, order_id
             * 
             * Method: markettrades (authenticated)
             * Present properties: tradeid, datetime, tradeprice, quantity, total, initiate_ordertype
             * 
             * Method: singlemarketdata (public)
             * Present properties: id, time, price, quantity, total
             * 
             */

            var trade = new Trade
            {
                TradeId = o.Value<Int64?>("id") ?? o.Value<Int64>("tradeid"),
                DateTimeUtc = TimeZoneInfo.ConvertTime(o.Value<DateTime?>("time") ?? o.Value<DateTime>("datetime"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc),
                UnitPrice = o.Value<decimal ?>("price") ?? o.Value<decimal>("tradeprice"),
                Quantity = o.Value<decimal>("quantity"),
                TotalPrice = o.Value<decimal>("total"),
                Fee = o.Value<decimal?>("fee") ?? 0,

                //If not present: UNKNOWN; if present: Buy or Sell
                InitiateOrderType = o.Value<String>("initiate_ordertype") == null ? CryptsyOrder.ORDER_TYPE.NA : (o.Value<String>("initiate_ordertype").ToLower() == "buy" ? CryptsyOrder.ORDER_TYPE.BUY : CryptsyOrder.ORDER_TYPE.SELL),
                TradeType = o.Value<String>("tradetype") == null ? CryptsyOrder.ORDER_TYPE.NA : (o.Value<String>("tradetype").ToLower() == "buy" ? CryptsyOrder.ORDER_TYPE.BUY : CryptsyOrder.ORDER_TYPE.SELL),
                OrderId = o.Value<Int64?>("order_id") ?? -1
            };

            return trade;
        }


        public static List<Trade> ReadMultipleFromJArray(JArray array)
        {
            if (array == null)
                return new List<Trade>();

            return (from JObject o in array select Trade.ReadFromJObject(o)).ToList();
        }
    }
}
