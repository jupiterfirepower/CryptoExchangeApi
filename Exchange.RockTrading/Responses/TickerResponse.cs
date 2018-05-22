using System.Collections.Generic;
using Exchange.RockTrading.Model;
using Newtonsoft.Json;

namespace Exchange.RockTrading.Responses
{
    public class TickerResponse
    {
        [JsonProperty(PropertyName = "result")]
        public List<RockTradingTicker> Result { get; set; }
    }
}
