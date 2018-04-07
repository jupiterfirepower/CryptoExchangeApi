using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Bter.Model
{
    public class BterMarketInfo
    {
        [JsonProperty(PropertyName = "no")]
        public int No { get; set; }
        [JsonProperty(PropertyName = "symbol")]
        public string Symbol { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "name_cn")]
        public string NameCn { get; set; }

        [JsonProperty(PropertyName = "pair")]
        public string Pair { get; set; }

        [JsonProperty(PropertyName = "rate")]
        public decimal Rate { get; set; }

        [JsonProperty(PropertyName = "vol_a")]
        public decimal BaseVol { get; set; }
        [JsonProperty(PropertyName = "vol_b")]
        public decimal MarketVol { get; set; }

        [JsonProperty(PropertyName = "curr_a")]
        public string BaseCurrency { get; set; }
        [JsonProperty(PropertyName = "curr_b")]
        public string MarketCurrency { get; set; }

        [JsonProperty(PropertyName = "curr_suffix")]
        public string CurrencySuffix { get; set; }

        [JsonProperty(PropertyName = "rate_percent")]
        public float RatePercent { get; set; }

        [JsonProperty(PropertyName = "trend")]
        public string Trend { get; set; }

        [JsonProperty(PropertyName = "supply")]
        public long Supply { get; set; }

        [JsonProperty(PropertyName = "marketcap")]
        public decimal MarketCap { get; set; }

        [JsonIgnore]
        public List<Tuple<long, decimal>> Plot { get; set; }

        public static ConcurrentBag<BterMarketInfo> GetFromJObject(JObject o)
        {
            var resultData = new ConcurrentBag<BterMarketInfo>();
            o.Last.First.Children<JObject>().Select(GetSingleFromJObject).ToList().ForEach(resultData.Add);
            return resultData;
        }

        private static BterMarketInfo GetSingleFromJObject(JObject o)
        {
            var marketInfo = o.ToObject<BterMarketInfo>();
            marketInfo.Plot = o["plot"].Select(content => Tuple.Create(Convert.ToInt64(content.First), Convert.ToDecimal(content.Last))).ToList();
            return marketInfo;
        }
    }
}
