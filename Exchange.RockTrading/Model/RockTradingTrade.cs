using Newtonsoft.Json;

namespace Exchange.RockTrading.Model
{
    public class RockTradingTrade
    {
        [JsonProperty(PropertyName = "date")]
        public int Date { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "tid")]
        public int Tid { get; set; }
    }
}
