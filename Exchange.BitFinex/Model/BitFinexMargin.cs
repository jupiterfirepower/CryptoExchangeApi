using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexMargin
    {
        [JsonProperty("margin_balance")]
        public string MarginBalance { get; set; }
        [JsonProperty("tradable_balance")]
        public string TradableBalance { get; set; }
        [JsonProperty("unrealized_pl")]
        public int UnrealizedPl { get; set; }
        [JsonProperty("unrealized_swap")]
        public int UnrealizedSwap { get; set; }
        [JsonProperty("net_value")]
        public string NetValue { get; set; }
        [JsonProperty("required_margin")]
        public int RequiredMargin { get; set; }
        [JsonProperty("leverage")]
        public string Leverage { get; set; }
        [JsonProperty("margin_requirement")]
        public string MarginRequirement { get; set; }
        [JsonProperty("margin_limits")]
        public List<BitFinexMarginLimit> MarginLimits { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
    }
}
