using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Bter.Model
{
    public class BterOrder
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "sell_type")]
        public string SellType { get; set; }

        [JsonProperty(PropertyName = "buy_type")]
        public string BuyType { get; set; }

        [JsonProperty(PropertyName = "sell_amount")]
        public decimal SellAmount { get; set; }

        [JsonProperty(PropertyName = "buy_amount")]
        public decimal BuyAmount { get; set; }

        [JsonProperty(PropertyName = "pair")]
        public string Pair { get; set; }

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        [JsonProperty(PropertyName = "rate")]
        public decimal Rate { get; set; }

        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }

        [JsonProperty(PropertyName = "initial_rate")]
        public decimal InitialRate { get; set; }

        [JsonProperty(PropertyName = "initial_amount")]
        public decimal InitialAmount { get; set; }

        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

        public static ConcurrentBag<BterOrder> GetFromJObject(JObject o)
        {
            var resultList = new ConcurrentBag<BterOrder>();
            o["orders"].Select(x=>x.ToObject<BterOrder>()).ToList().ForEach(resultList.Add);
            return resultList;
        }
    }
}
