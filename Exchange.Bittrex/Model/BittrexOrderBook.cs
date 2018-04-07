using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Bittrex.Model
{
    public class BittrexOrderBook
    {
        [JsonProperty("buy")]
        public List<BittrexOrder> buy { get; set; }
        [JsonProperty("sell")]
        public List<BittrexOrder> sell { get; set; }
    }
}
