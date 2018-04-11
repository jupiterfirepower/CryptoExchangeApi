using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Exchange.Poloniex.Model
{
    public class PoloniexBalance
    {
        public string Currency { get; set; }

        public decimal Value { get; set; }

        public static IEnumerable<PoloniexBalance> GetFromJObject(JObject o)
        {
            var resultList = new List<PoloniexBalance>();
            o.OfType<JProperty>().Select(x => new PoloniexBalance { Currency = x.Name , Value = x.Value.Value<decimal>()}).ToList().ForEach(resultList.Add);
            return resultList;
        }
    }
}
