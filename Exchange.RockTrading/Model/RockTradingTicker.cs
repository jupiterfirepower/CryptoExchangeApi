using Newtonsoft.Json;

namespace Exchange.RockTrading.Model
{
    public class RockTradingTicker
    {
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }
        [JsonProperty(PropertyName = "bid")]
        public string Bid { get; set; }
        [JsonProperty(PropertyName = "ask")]
        public string Ask { get; set; }
        [JsonProperty(PropertyName = "last")]
        public string Last { get; set; }
        [JsonProperty(PropertyName = "volume")]
        public string Volume { get; set; }
        [JsonProperty(PropertyName = "volume_traded")]
        public string VolumeTraded { get; set; }
        [JsonProperty(PropertyName = "open")]
        public string Open { get; set; }
        [JsonProperty(PropertyName = "high")]
        public string High { get; set; }
        [JsonProperty(PropertyName = "low")]
        public string Low { get; set; }
        [JsonProperty(PropertyName = "close")]
        public string Close { get; set; }
    }
}
