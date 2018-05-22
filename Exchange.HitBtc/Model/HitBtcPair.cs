using Newtonsoft.Json;

namespace Exchange.HitBtc.Model
{
    public class HitBtcPair
    {
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        [JsonProperty("step")]
        public string Step { get; set; }
        [JsonProperty("lot")]
        public string Lot { get; set; }
        [JsonProperty("currency")]
        public string Currency { get; set; }
        [JsonProperty("commodity")]
        public string Commodity { get; set; }
        [JsonProperty("takeLiquidityRate")]
        public string TakeLiquidityRate { get; set; }
        [JsonProperty("provideLiquidityRate")]
        public string ProvideLiquidityRate { get; set; }        
    }
}
