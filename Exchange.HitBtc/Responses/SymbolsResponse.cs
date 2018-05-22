using System.Collections.Generic;
using Exchange.HitBtc.Model;
using Newtonsoft.Json;

namespace Exchange.HitBtc.Responses
{
    public class SymbolsResponse
    {
        [JsonProperty("symbols")]
        public List<HitBtcPair> Symbols { get; set; }
    }
}
