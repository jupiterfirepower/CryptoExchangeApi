using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexTicker
    {
        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty(PropertyName = "bid")]
        public decimal Bid { get; set; }
        [JsonProperty(PropertyName = "ask")]
        public decimal Ask { get; set; }
        [JsonProperty(PropertyName = "low")]
        public string Low { get; set; }
        [JsonProperty(PropertyName = "high")]
        public string High { get; set; }
        [JsonProperty(PropertyName = "last")]
        public string Last { get; set; }
        [JsonProperty(PropertyName = "volume")]
        public string Volume { get; set; }
        [JsonProperty(PropertyName = "volume30d")]
        public string Volume30D { get; set; }
    }
}
