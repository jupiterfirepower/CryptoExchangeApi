using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexBidAskBase
    {
        [JsonProperty("rate")]
        public decimal Rate { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("period")]
        public int Period { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty("frr")]
        public string Frr { get; set; }
    }
}
