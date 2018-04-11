using Newtonsoft.Json;

namespace Exchange.Poloniex.Responses
{
    public class CancelOrderResponse : PoloniexResponse
    {
        [JsonProperty(PropertyName = "success")]
        public int Success { get; set; }
    }
}
