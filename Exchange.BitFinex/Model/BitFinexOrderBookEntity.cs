using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinixOrder
    {
        [JsonProperty("price")]
        public decimal Price { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
