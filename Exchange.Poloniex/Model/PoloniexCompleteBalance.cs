using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Poloniex.Model
{
    public class PoloniexCompleteBalance
    {
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "available")]
        public decimal Available { get; set; }
        [JsonProperty(PropertyName = "onOrders")]
        public decimal OnOrders { get; set; }
        [JsonProperty(PropertyName = "btcValue")]
        public decimal BtcValue { get; set; }


        public static IEnumerable<PoloniexCompleteBalance> GetFromJObject(JObject o)
        {
            var resultList = new List<PoloniexCompleteBalance>();
            o.OfType<JProperty>().Select(x =>
            {
                var tmp = x.Value.ToObject<PoloniexCompleteBalance>();
                tmp.Currency = x.Name;
                return tmp;
            }).ToList().ForEach(resultList.Add);
            return resultList;
        }
    }
}
