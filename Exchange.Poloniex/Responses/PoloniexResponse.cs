using Newtonsoft.Json;

namespace Exchange.Poloniex.Responses
{
    public class PoloniexResponse
    {
       [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
