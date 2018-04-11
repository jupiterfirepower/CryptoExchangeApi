using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Poloniex.Model
{
    public class PoloniexOrderBook
    {
        [JsonProperty(PropertyName = "asks")]
        public List<List<decimal>> Asks { get; set; }
        [JsonProperty(PropertyName = "bids")]
        public List<List<decimal>> Bids { get; set; }
        [JsonProperty(PropertyName = "isFrozen")]
        public string IsFrozen { get; set; }
    }
}
