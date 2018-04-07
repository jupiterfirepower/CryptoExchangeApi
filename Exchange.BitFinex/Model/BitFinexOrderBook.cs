using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexOrderBook
    {
        [JsonProperty("bids")]
        public List<BitFinixOrder> Bids { get; set; }
        [JsonProperty("asks")]
        public List<BitFinixOrder> Asks { get; set; }
    }
}
