using Incryptex.Common.Contracts;
using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexOrderArchive
    {
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "symbol1")]
        public string Symbol1 { get; set; }
        [JsonProperty(PropertyName = "symbol2")]
        public string Symbol2 { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "remains")]
        public decimal Remains { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "time")]
        public string Time { get; set; }
        [JsonProperty(PropertyName = "tradingFeeBuy")]
        public decimal TradingFeeBuy { get; set; }
        [JsonProperty(PropertyName = "tradingFeeSell")]
        public decimal TradingFeeSell { get; set; }
        [JsonProperty(PropertyName = "orderId")]
        public string OrderId { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        public OrderStatus OrderStatus
        {
            get
            {
                switch (Status)
                {
                    case "d":
                        return OrderStatus.Filled;
                    case "c":
                        return OrderStatus.Canceled;
                    default:
                        return OrderStatus.Unknown;
                }
            }
        }

    }
}
