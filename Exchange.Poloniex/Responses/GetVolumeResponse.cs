using System.Collections.Generic;
using Exchange.Poloniex.Model;
using Newtonsoft.Json;

namespace Exchange.Poloniex.Responses
{
    public class GetVolumeResponse : PoloniexResponse
    {
        [JsonProperty(PropertyName = "totalBTC")]
        public string TotalBtc { get; set; }
        [JsonProperty(PropertyName = "totalUSDT")]
        public string TotalUsdt { get; set; }
        [JsonProperty(PropertyName = "totalXMR")]
        public string TotalXmr { get; set; }
        [JsonProperty(PropertyName = "totalXUSD")]
        public string TotalXusd { get; set; }

        public IEnumerable<PoloniexVolume> Volumes { get; set; }
    }
}
