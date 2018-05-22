
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Kraken.Model
{  
    public class KrakenAssetPair
    {
        [JsonProperty(PropertyName = "altname")]
        public string AltName { get; set; }
        [JsonProperty(PropertyName = "aclass_base")]
        public string AClassBase { get; set; }
        [JsonProperty(PropertyName = "base")]
        public string Base { get; set; }
        [JsonProperty(PropertyName = "aclass_quote")]
        public string AclassQuote { get; set; }
        [JsonProperty(PropertyName = "quote")]
        public string Quote { get; set; }
        [JsonProperty(PropertyName = "lot")]
        public string Lot { get; set; }
        [JsonProperty(PropertyName = "pair_decimals")]
        public int PairDecimals { get; set; }
        [JsonProperty(PropertyName = "lot_decimals")]
        public int LotDecimals { get; set; }
        [JsonProperty(PropertyName = "lot_multiplier")]
        public int LotMultiplier { get; set; }
        [JsonProperty(PropertyName = "leverage")]
        public List<object> Leverage { get; set; }
        [JsonProperty(PropertyName = "fees")]
        public List<List<double>> Fees { get; set; }
        [JsonProperty(PropertyName = "fee_volume_currency")]
        public string FeeVolumeCurrency { get; set; }
        [JsonProperty(PropertyName = "margin_call")]
        public int MarginCall { get; set; }
        [JsonProperty(PropertyName = "margin_stop")]
        public int MarginStop { get; set; }
    }

    public class KrakenAssetPairs
    {
        public Dictionary<string, KrakenAssetPair> List { get; private set; }

        public static KrakenAssetPairs ReadFromJObject(JObject o)
        {            
            return new KrakenAssetPairs()
            {
                List = o.ToObject<Dictionary<string, JToken>>().ToDictionary(item => item.Key, item => item.Value.ToObject<KrakenAssetPair>())
            };            
        }
    }
}
