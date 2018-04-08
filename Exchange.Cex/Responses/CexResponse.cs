using Newtonsoft.Json;

namespace Exchange.Cex.Responses
{
    public class CexResponse
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
