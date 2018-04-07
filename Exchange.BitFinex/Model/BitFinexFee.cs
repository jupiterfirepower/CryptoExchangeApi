using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexFee
    {
        [JsonProperty("pair")]
        public string Pairs { get; set; }
        [JsonProperty("maker_fees")]
        public decimal MakerFees { get; set; }
        [JsonProperty("taker_fees")]
        public decimal TakerFees { get; set; }
    }
}
