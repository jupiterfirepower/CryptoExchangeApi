using Newtonsoft.Json;

namespace Exchange.Bitstamp.Model
{
    public class BitStampUserTransaction
    {
        [JsonProperty(PropertyName = "usd")]
        public decimal Usd { get; set; }
        [JsonProperty(PropertyName = "btc")]
        public decimal Btc { get; set; }
        [JsonProperty(PropertyName = "btc_usd")]
        public decimal BtcUsd { get; set; }
        [JsonProperty(PropertyName = "order_id")]
        public int? OrderId { get; set; }
        [JsonProperty(PropertyName = "fee")]
        public decimal Fee { get; set; }
        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "datetime")]
        public string DateTime { get; set; }
    }
}
