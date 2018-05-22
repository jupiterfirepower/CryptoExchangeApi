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
    public class CryptsyOrder
    {
        [JsonProperty(PropertyName = "orderid")]
        public string OrderId { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "quantity")]
        public decimal Quantity { get; set; }
        [JsonProperty(PropertyName = "total")]
        public decimal Total { get; set; }
        [JsonProperty(PropertyName = "ordertype")]
        public string OrderTypeOriginal { get; set; }
        public DateTime? CreatedUtc { get; private set; }
        [JsonProperty(PropertyName = "orig_quantity")]
        public decimal OriginalQuantity { get; private set; } //Original Total CryptsyOrder Quantity
        [JsonProperty(PropertyName = "marketid")]
        public Int64 MarketId { get; private set; }

        public CryptsyOrderType OrderType { get; set; }


        public static CryptsyOrder ReadFromJObject(JObject o, Int64 marketId = -1, CryptsyOrderType orderType = CryptsyOrderType.Na)
        {
            if (o == null)
                return null;

            var order = new CryptsyOrder
            {
                Price = o.Value<decimal>("price"),
                Quantity = o.Value<decimal>("quantity"),
                Total = o.Value<decimal>("total"),
                OriginalQuantity = o.Value<decimal?>("orig_quantity") ?? -1,
                MarketId = o.Value<Int64?>("marketid") ?? marketId,

                //If ordertype is present, use it, if not: use the ordertype passed to the method
                OrderType =
                    o.Value<string>("ordertype") == null
                        ? orderType
                        : (o.Value<string>("ordertype").ToLower() == "buy" ? CryptsyOrderType.Buy : CryptsyOrderType.Sell),
                CreatedUtc = o.Value<DateTime?>("created")
            };

            if (order.CreatedUtc != null)
                order.CreatedUtc = TimeZoneInfo.ConvertTime((DateTime)order.CreatedUtc, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc); //Convert to UTC

            return order;
        }

        public static List<CryptsyOrder> ReadMultipleFromJArray(JArray array, Int64 marketId = -1)
        {
            var orders = new List<CryptsyOrder>();

            if (array == null)
                return orders; //empty list, if nothing to read

            orders.AddRange(from JObject o in array select ReadFromJObject(o, marketId));

            return orders;
        }
    }
}
