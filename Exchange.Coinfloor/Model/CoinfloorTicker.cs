using Newtonsoft.Json;

namespace Exchange.Coinfloor.Model
{
    public class CoinfloorTicker
    {
        [JsonProperty(PropertyName = "last")]
        public decimal Last { get; set; }
        [JsonProperty(PropertyName = "high")]
        public decimal High { get; set; }
        [JsonProperty(PropertyName = "low")]
        public decimal Low { get; set; }
        [JsonProperty(PropertyName = "vwap")]
        public decimal Vwap { get; set; }
        [JsonProperty(PropertyName = "volume")]
        public decimal Volume { get; set; }
        [JsonProperty(PropertyName = "bid")]
        public decimal Bid { get; set; }
        [JsonProperty(PropertyName = "ask")]
        public decimal Ask { get; set; }
    }
}
