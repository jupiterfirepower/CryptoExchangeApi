using Newtonsoft.Json;

namespace Exchange.Coinfloor.Model
{
    public class CoinfloorTransaction
    {
        [JsonProperty(PropertyName = "date")]
        public int Date { get; set; }
        [JsonProperty(PropertyName = "tid")]
        public long Tid { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
    }
}
