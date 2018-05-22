using Newtonsoft.Json;

namespace Exchange.Bitstamp.Model
{
    public class BitStampConversion
    {
        [JsonProperty(PropertyName = "buy")]
        public string Buy;
        [JsonProperty(PropertyName = "sell")]
        public string Sell;
    }
}
