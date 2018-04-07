using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexTicker
    {
        [JsonProperty("mid")]
        public decimal Mid { get; set; }
        [JsonProperty("bid")]
        public decimal Bid { get; set; }
        [JsonProperty("ask")]
        public decimal Ask { get; set; }
        [JsonProperty("last_price")]
        public decimal LastPrice { get; set; }
        [JsonProperty("low")]
        public decimal Low { get; set; }
        [JsonProperty("high")]
        public decimal High { get; set; }
        [JsonProperty("volume")]
        public decimal Volume { get; set; }
        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }
    }
}
