using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.RockTrading.Model
{
    public class RockTradingOrderBook
    {
        [JsonProperty(PropertyName = "asks")]
        public List<List<decimal>> Asks { get; set; }
        [JsonProperty(PropertyName = "bids")]
        public List<List<decimal>> Bids { get; set; }
    }
}
