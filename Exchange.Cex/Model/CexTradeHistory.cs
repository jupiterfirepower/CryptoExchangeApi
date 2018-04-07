using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexTradeHistory
    {
        [JsonProperty(PropertyName = "tid")]
        public string Tid { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "date")]
        public string Date { get; set; }
    }
}
