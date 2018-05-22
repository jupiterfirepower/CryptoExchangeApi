using Newtonsoft.Json;

namespace Exchange.Bitstamp.Model
{
    public class BitStampReserve
    {
        [JsonProperty(PropertyName = "usd")]
        public string Usd;
    }
}
