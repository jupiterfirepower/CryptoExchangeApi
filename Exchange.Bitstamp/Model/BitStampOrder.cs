using Newtonsoft.Json;

namespace Exchange.Bitstamp.Model
{
    public class BitStampOrder : BitStampBase
    {
        [JsonProperty(PropertyName= "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName= "datetime")]
        public string Datetime { get; set; }
        [JsonProperty(PropertyName= "type")]
        public int Type { get; set; }
        [JsonProperty(PropertyName= "price")]
        public decimal Price { get; set; }
        [JsonProperty(PropertyName= "amount")]
        public decimal Amount { get; set; }
    }
}
