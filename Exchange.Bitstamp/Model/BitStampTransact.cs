using Newtonsoft.Json;

namespace Exchange.Bitstamp.Model
{
    public class BitStampTransact
    {
        [JsonProperty(PropertyName = "date")]
        public string Date;
        [JsonProperty(PropertyName = "tid")]
        public string Tid;
        [JsonProperty(PropertyName = "price")]
        public string Price;
        [JsonProperty(PropertyName = "amount")]
        public string Amount;
    }
}
