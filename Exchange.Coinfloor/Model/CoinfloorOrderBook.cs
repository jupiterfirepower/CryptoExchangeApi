using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Coinfloor.Model
{
    public class CoinfloorOrderBook
    {
        [JsonProperty(PropertyName = "bids")]
        public List<List<decimal>> Bids { get; set; }
        [JsonProperty(PropertyName = "asks")]
        public List<List<decimal>> Asks { get; set; }
    }
}
