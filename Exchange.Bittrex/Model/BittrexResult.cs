using Newtonsoft.Json;

namespace Exchange.Bittrex.Model
{
    public class BittrexResult
    {
        [JsonProperty("uuid")]
        public string Uuid { get; set; }
    }
}
