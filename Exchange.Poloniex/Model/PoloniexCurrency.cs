using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Poloniex.Model
{
    public class PoloniexCurrency
    {
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "maxDailyWithdrawal")]
        public decimal MaxDailyWithdrawal { get; set; }
        [JsonProperty(PropertyName = "txFee")]
        public decimal Fee { get; set; }
        [JsonProperty(PropertyName = "minConf")]
        public int MinConf { get; set; }
        [JsonProperty(PropertyName = "disabled")]
        public int Disabled { get; set; }
        [JsonProperty(PropertyName = "delisted")]
        public int Delisted { get; set; }

        public static IEnumerable<PoloniexCurrency> GetFromJObject(JObject o)
        {
            var resultList = new List<PoloniexCurrency>();
            o.OfType<JProperty>().Select(x =>
            {
                var tmp = x.Value.ToObject<PoloniexCurrency>();
                tmp.Currency = x.Name;
                return tmp;
            }).ToList().ForEach(resultList.Add);
            return resultList;
        }
    }
}
