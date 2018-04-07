using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexTrade
    {
        [JsonProperty("timestamp")]
        public int Timestamp { get; set; }
        [JsonProperty("tid")]
        public int Tid { get; set; }
        [JsonProperty("price")]
        public string Price { get; set; }
        [JsonProperty("amount")]
        public string Amount { get; set; }
        [JsonProperty("exchange")]
        public string Exchange { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
    }
}
