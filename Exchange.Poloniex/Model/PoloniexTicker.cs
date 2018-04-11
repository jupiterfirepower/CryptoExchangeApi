using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Poloniex.Model
{
    public class PoloniexTicker
    {
        public string PairStr { get; set; }
        [JsonProperty(PropertyName = "last")]
        public decimal Last { get; set; }
        [JsonProperty(PropertyName = "lowestAsk")]
        public decimal LowestAsk { get; set; }
        [JsonProperty(PropertyName = "highestBid")]
        public decimal HighestBid { get; set; }
        [JsonProperty(PropertyName = "percentChange")]
        public decimal PercentChange { get; set; }
        [JsonProperty(PropertyName = "baseVolume")]
        public decimal BaseVolume { get; set; }
        [JsonProperty(PropertyName = "quoteVolume")]
        public decimal QuoteVolume { get; set; }
        [JsonProperty(PropertyName = "isFrozen")]
        public string IsFrozenData { get; set; }
        [JsonProperty(PropertyName = "high24hr")]
        public decimal High24Hr { get; set; }
        [JsonProperty(PropertyName = "low24hr")]
        public decimal Low24Hr { get; set; }

        public bool IsFrozen { get { return IsFrozenData == "1"; } }

        public static IEnumerable<PoloniexTicker> GetFromJObject(JObject o)
        {
            var resultList = new List<PoloniexTicker>();
            o.OfType<JProperty>().Select(x =>
            {
                var tmp = x.Value.ToObject<PoloniexTicker>();
                tmp.PairStr = x.Name;
                return tmp;
            }).ToList().ForEach(resultList.Add);
            return resultList;
        }
    }
}
