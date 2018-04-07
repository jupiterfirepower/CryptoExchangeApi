using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexLandBook
    {
        [JsonProperty("bids")]
        public List<BitFinexBidAskBase> Bids { get; set; }
        [JsonProperty("asks")]
        public List<BitFinexBidAskBase> Asks { get; set; }
    }
}
