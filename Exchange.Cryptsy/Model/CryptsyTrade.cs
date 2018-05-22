/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Exchange.Cryptsy.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Cryptsy.Model
{
    public class CryptsyTrade
    {
        [JsonProperty("tradeid")]
        public long TradeId { get; set; }
        [JsonProperty("datetime")]
        public DateTime DateTimeUtc { get; set; }
        [JsonProperty("marketid")]
        public int? MarketId { get; set; }
        [JsonProperty("price")]
        public decimal UnitPrice { get; set; }
        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }
        [JsonProperty("total")]
        public decimal Total { get; set; }
        public CryptsyOrderType InitiateOrderType { get; set; }
        public CryptsyOrderType TradeType { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        public long OrderId { get; set; } //Original order id this trade was executed against. -1 if not applicable
        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        public static CryptsyTrade ReadFromJObject(JObject o)
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

            var trade = new CryptsyTrade
            {
                TradeId = o.Value<Int64?>("id") ?? o.Value<Int64>("tradeid"),
                DateTimeUtc = TimeZoneInfo.ConvertTime(o.Value<DateTime?>("time") ?? o.Value<DateTime>("datetime"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc),
                UnitPrice = o.Value<decimal>("tradeprice"),
                Quantity = o.Value<decimal>("quantity"),
                Total = o.Value<decimal>("total"),
                Fee = o.Value<decimal?>("fee") ?? 0,

                //If not present: UNKNOWN; if present: Buy or Sell
                InitiateOrderType = o.Value<String>("initiate_ordertype") == null ? CryptsyOrderType.Na : (o.Value<String>("initiate_ordertype").ToLower() == "buy" ? CryptsyOrderType.Buy : CryptsyOrderType.Sell),
                TradeType = o.Value<String>("tradetype") == null ? CryptsyOrderType.Na : (o.Value<String>("tradetype").ToLower() == "buy" ? CryptsyOrderType.Buy : CryptsyOrderType.Sell),
                OrderId = o.Value<Int64?>("order_id") ?? -1
            };

            try
            {
                trade.MarketId = o.Value<int?>("marketid");
            }
            catch
            {
                // ignored
            }

            return trade;
        }


        public static List<CryptsyTrade> ReadMultipleFromJArray(JArray array)
        {
            if (array == null)
                return new List<CryptsyTrade>();

            return (from JObject o in array select ReadFromJObject(o)).ToList();
        }
    }
}
