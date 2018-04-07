using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexWalletBalance
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("amount")]
        public decimal Amount { get; set; }
        [JsonProperty("available")]
        public decimal Available { get; set; }
    }
}
