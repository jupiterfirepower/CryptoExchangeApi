using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Cryptsy.Model
{
    public class CryptsyOrderStatusInfo
    {
        [JsonProperty(PropertyName = "active")]
        public bool Active { get; set; }

        [JsonProperty(PropertyName = "remainqty")]
        public decimal RemainQty { get; set; } 

        [JsonProperty(PropertyName = "tradeinfo")]
        public List<CryptsyTrade> TradeInfo { get; set; } 
    }
}
