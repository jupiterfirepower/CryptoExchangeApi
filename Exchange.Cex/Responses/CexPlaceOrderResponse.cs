using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Response
{
    public class CexPlaceOrderResponse : CexResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName = "amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "pending")]
        public decimal PendingAmount { get; set; }
    }
}
