using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Bter.Model
{
    public class BterOrderBook
    {
        [JsonProperty(PropertyName = "asks")]
        public List<string[]> Asks { get; set; }
        [JsonProperty(PropertyName = "bids")]
        public List<string[]> Bids { get; set; }
    }
}
