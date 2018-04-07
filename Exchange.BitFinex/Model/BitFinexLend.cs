using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexLend
    {
        [JsonProperty("rate")]
        public decimal Rate { get; set; }
        [JsonProperty("amount_lent")]
        public decimal AmountLent { get; set; }
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
    }
}
