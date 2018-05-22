
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Kraken.Model
{
    /*
    a = ask array(<price>, <lot volume>),
    b = bid array(<price>, <lot volume>),
    c = last trade closed array(<price>, <lot volume>),
    v = volume array(<today>, <last 24 hours>),
    p = volume weighted average price array(<today>, <last 24 hours>),
    t = number of trades array(<today>, <last 24 hours>),
    l = low array(<today>, <last 24 hours>),
    h = high array(<today>, <last 24 hours>),
    o = today's opening price
     */

    public class KrakenTicket
    {
        [JsonProperty(PropertyName = "a")]
        public List<string> Asks { get; set; }
        [JsonProperty(PropertyName = "b")]
        public List<string> Bids { get; set; }
        [JsonProperty(PropertyName = "c")]
        public List<string> LastTrade { get; set; }
        [JsonProperty(PropertyName = "v")]
        public List<string> Volume { get; set; }
        [JsonProperty(PropertyName = "p")]
        public List<string> VolumeAveragePrice { get; set; }
        [JsonProperty(PropertyName = "t")]
        public List<int> TradeNumber { get; set; }
        [JsonProperty(PropertyName = "l")]
        public List<string> Low { get; set; }
        [JsonProperty(PropertyName = "h")]
        public List<string> High { get; set; }
        [JsonProperty(PropertyName = "o")]
        public string Price { get; set; }
    }
}
