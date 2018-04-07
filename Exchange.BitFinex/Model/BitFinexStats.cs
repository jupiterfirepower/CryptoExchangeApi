using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexStats
    {
        [JsonProperty("period")]
        public int Period { get; set; }
        [JsonProperty("volume")]
        public decimal Volume { get; set; }
    }
}
