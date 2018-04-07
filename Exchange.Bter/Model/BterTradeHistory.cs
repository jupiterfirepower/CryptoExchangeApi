using Newtonsoft.Json;

namespace Exchange.Bter.Model
{
    public class BterTradeHistory
    {
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "tid")]
        public string Tid { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
    }
}
