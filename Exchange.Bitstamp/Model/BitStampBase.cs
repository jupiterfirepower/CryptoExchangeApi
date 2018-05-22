using Newtonsoft.Json;

namespace Exchange.Bitstamp.Model
{
    public class BitStampBase
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
