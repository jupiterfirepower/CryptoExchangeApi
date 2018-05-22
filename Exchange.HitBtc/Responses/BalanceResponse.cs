using System.Collections.Generic;
using Exchange.HitBtc.Model;
using Newtonsoft.Json;

namespace Exchange.HitBtc.Responses
{
    public class TradingBalanceResponse
    {
        [JsonProperty("balance")]
        public List<HitBtcBalance> Balance { get; set; }
    }
}
