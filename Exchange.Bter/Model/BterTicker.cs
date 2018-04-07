using System.Collections.Concurrent;
using System.Linq;
using Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Bter.Model
{
    public class BterTicker
    {
        public Pair Pair { get; set; }

        public string PairStr { get; set; }

        [JsonProperty(PropertyName = "last")]
        public decimal Last { get; set; }
        [JsonProperty(PropertyName = "high")]
        public decimal High { get; set; }
        [JsonProperty(PropertyName = "low")]
        public decimal Low { get; set; }
        [JsonProperty(PropertyName = "avg")]
        public decimal Avg { get; set; }
        [JsonProperty(PropertyName = "sell")]
        public decimal Sell { get; set; }
        [JsonProperty(PropertyName = "buy")]
        public decimal Buy { get; set; }
        [JsonProperty(PropertyName = "vol_btc")]
        public decimal VolFrom { get; set; }
        [JsonProperty(PropertyName = "vol_cny")]
        public decimal VolTo { get; set; }

        public static ConcurrentBag<BterTicker> GetFromJObject(JObject o)
        {
            var resultList = new ConcurrentBag<BterTicker>();
            o.OfType<JProperty>().Select(x => 
            { 
                var tmp = x.Value.ToObject<BterTicker>();
                tmp.PairStr = x.Name;
                return tmp;
            }).ToList().ForEach(resultList.Add);
            return resultList;
        }
    }
}
