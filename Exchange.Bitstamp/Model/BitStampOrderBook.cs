using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Bitstamp.Model
{
    public class BitStampOrderBook
    {
        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty(PropertyName = "bids")]
        public List<string[]> Bids { get; set; }
        [JsonProperty(PropertyName = "asks")]
        public List<string[]> Asks { get; set; }
    }
}
