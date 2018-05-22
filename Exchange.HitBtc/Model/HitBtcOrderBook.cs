using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.HitBtc.Model
{
    public class HitBtcOrderBook
    {
        [JsonProperty(PropertyName = "asks")]
        public List<List<decimal>> Asks { get; set; }
        [JsonProperty(PropertyName = "bids")]
        public List<List<decimal>> Bids { get; set; }
    }
}
