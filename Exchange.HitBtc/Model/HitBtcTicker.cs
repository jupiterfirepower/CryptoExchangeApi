using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.HitBtc.Model
{
    public class HitBtcTicker
    {
        public Pair Pair { get; set; }

        public string PairStr { get; set; }

        [JsonProperty(PropertyName = "ask")]
        public decimal Ask { get; set; }

        [JsonProperty(PropertyName = "bid")]
        public decimal Bid { get; set; }

        [JsonProperty(PropertyName = "last")]
        public decimal Last { get; set; }

        [JsonProperty(PropertyName = "low")]
        public decimal Low { get; set; }

        [JsonProperty(PropertyName = "high")]
        public decimal High { get; set; }

        [JsonProperty(PropertyName = "volume")]
        public decimal Volume { get; set; }

        [JsonProperty(PropertyName = "open")]
        public decimal Open { get; set; }

        [JsonProperty(PropertyName = "volume_quote")]
        public decimal VolumeQuote { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public long Timestamp { get; set; }

        public static IEnumerable<HitBtcTicker> GetFromJObject(JObject o)
        {
            var resultList = new List<HitBtcTicker>();
            o.OfType<JProperty>().Select(x =>
            {
                var tmp = x.Value.ToObject<HitBtcTicker>();
                tmp.PairStr = x.Name;
                return tmp;
            }).ToList().ForEach(resultList.Add);
            return resultList;
        }
        
    }
}
