
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Exchange.Kraken.Model
{
    public class KrakenAsset
    {
        [JsonProperty(PropertyName = "aclass")]
        public string AClass { get; set; }
        [JsonProperty(PropertyName = "altname")]
        public string AltName { get; set; }
        [JsonProperty(PropertyName = "decimals")]
        public int Decimals { get; set; }
        [JsonProperty(PropertyName = "display_decimals")]
        public int DisplayDecimals { get; set; }
    }

    public class KrakenAssets
    {
        public Dictionary<string, KrakenAsset> List { get; private set; }
        public static KrakenAssets ReadFromJObject(JObject o)
        {
            return new KrakenAssets()
            {                
                List = o.ToObject<Dictionary<string, JToken>>().ToDictionary(item => item.Key, item => item.Value.ToObject<KrakenAsset>())
            };
        }
    }
}
