using Newtonsoft.Json;

namespace Exchange.ItBit.Model
{
    public class ItBitTicker
    {
        [JsonProperty(PropertyName = "pair")]
        public string Pair { get; set; }
        [JsonProperty(PropertyName = "bid")]
        public decimal Bid { get; set; }
        [JsonProperty(PropertyName = "bidAmt")]
        public decimal BidAmt { get; set; }
        [JsonProperty(PropertyName = "ask")]
        public decimal Ask { get; set; }
        [JsonProperty(PropertyName = "askAmt")]
        public decimal AskAmt { get; set; }
        [JsonProperty(PropertyName = "lastPrice")]
        public decimal LastPrice { get; set; }
        [JsonProperty(PropertyName = "lastAmt")]
        public decimal LastAmt { get; set; }
        [JsonProperty(PropertyName = "volume24h")]
        public decimal Volume24H { get; set; }
        [JsonProperty(PropertyName = "volumeToday")]
        public decimal VolumeToday { get; set; }
        [JsonProperty(PropertyName = "high24h")]
        public decimal High24H { get; set; }
        [JsonProperty(PropertyName = "low24h")]
        public decimal Low24H { get; set; }
        [JsonProperty(PropertyName = "highToday")]
        public decimal HighToday { get; set; }
        [JsonProperty(PropertyName = "lowToday")]
        public decimal LowToday { get; set; }
        [JsonProperty(PropertyName = "openToday")]
        public decimal OpenToday { get; set; }
        [JsonProperty(PropertyName = "vwapToday")]
        public decimal VwapToday { get; set; }
        [JsonProperty(PropertyName = "vwap24h")]
        public decimal Vwap24H { get; set; }
        [JsonProperty(PropertyName = "servertimeUTC")]
        public string ServerTimeUtc { get; set; }
    }
}
