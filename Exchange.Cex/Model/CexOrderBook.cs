using System.Collections.Generic;
using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexOrderBook
    {
        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty(PropertyName = "bids")]
        public List<List<decimal>> Bids { get; set; }
        [JsonProperty(PropertyName = "asks")]
        public List<List<decimal>> Asks { get; set; }
    }
}
