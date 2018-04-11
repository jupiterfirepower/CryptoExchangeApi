using Newtonsoft.Json;

namespace Exchange.Poloniex.Model
{
    public class PoloniexOrderHistory
    {
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "rate")]
        public decimal Rate { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "total")]
        public decimal Total { get; set; }
        [JsonProperty(PropertyName = "orderNumber")]
        public string OrderNumber { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
