
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Kraken.Model
{
    public class KrakenResponse
    {
        [JsonProperty(PropertyName = "error")]
        public IList<string> Error { get; set; }
        [JsonProperty(PropertyName = "result")]
        public object Result { get; set; }
    }
}
