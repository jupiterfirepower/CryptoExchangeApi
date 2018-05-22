using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.ItBit.Model
{
    public class ItBitOrderBook
    {
        [JsonProperty(PropertyName = "bids")]
        public List<List<decimal>> Bids { get; set; }
        [JsonProperty(PropertyName = "asks")]
        public List<List<decimal>> Asks { get; set; }
    }
}
