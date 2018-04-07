using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexMarginLimit
    {
        [JsonProperty("on_pair")]
        public string OnPair { get; set; }
        [JsonProperty("initial_margin")]
        public decimal InitialMargin { get; set; }
        [JsonProperty("margin_requirement")]
        public decimal MarginRequirement { get; set; }
        [JsonProperty("tradable_balance")]
        public decimal TradableBalance { get; set; }
    }
}
