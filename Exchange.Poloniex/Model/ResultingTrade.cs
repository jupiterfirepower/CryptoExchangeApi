using Newtonsoft.Json;

namespace Exchange.Poloniex.Model
{
    public class ResultingTrade
    {
        [JsonProperty(PropertyName = "amount")]
        public string Amount { get; set; }
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "rate")]
        public string Rate { get; set; }
        [JsonProperty(PropertyName = "total")]
        public string Total { get; set; }
        [JsonProperty(PropertyName = "tradeID")]
        public string TradeId { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
