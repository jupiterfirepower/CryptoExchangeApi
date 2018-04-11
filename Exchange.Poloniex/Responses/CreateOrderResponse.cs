using System.Collections.Generic;
using Exchange.Poloniex.Model;
using Newtonsoft.Json;

namespace Exchange.Poloniex.Responses
{
    public class CreateOrderResponse : PoloniexResponse
    {
        [JsonProperty(PropertyName = "orderNumber")]
        public int OrderNumber { get; set; }
        [JsonProperty(PropertyName = "resultingTrades")]
        public List<ResultingTrade> ResultingTrades { get; set; }
    }
}
