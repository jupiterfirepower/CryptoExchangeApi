using Newtonsoft.Json;

namespace Exchange.HitBtc.Model
{
    public class HitBtcBalance
    {
        [JsonProperty(PropertyName = "currency_code")]
        public string CurrencyCode { get; set; }
        [JsonProperty(PropertyName = "cash")]
        public int Cash { get; set; }
        [JsonProperty(PropertyName = "reserved")]
        public int Reserved { get; set; }
    }
}
