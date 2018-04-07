using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinixPosition
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("base")]
        public decimal Base { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("swap")]
        public string Swap { get; set; }
        [JsonProperty("pl")]
        public decimal Pl { get; set; }
    }
}
