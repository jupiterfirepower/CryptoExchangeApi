using Common.Contracts;
using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexPairDetails
    {
        private string _pairs;

        public Pair Pair { get; set; }
        [JsonProperty("pair")]
        public string Pairs 
        { 
            get { return _pairs; }
            set
            {
                _pairs = value;
                var data = value.ToChunks(3);
                Pair = new Pair(data[0].ToUpper(), data[1].ToUpper());
            } 
        }
        [JsonProperty("price_precision")]
        public int PricePrecision { get; set; }
        [JsonProperty("initial_margin")]
        public decimal InitialMargin { get; set; }
        [JsonProperty("minimum_margin")]
        public decimal MinimumMargin { get; set; }
        [JsonProperty("maximum_order_size")]
        public decimal MaximumOrderSize { get; set; }
        [JsonProperty("minimum_order_size")]
        public decimal MinimumOrderSize { get; set; }
        [JsonProperty("expiration")]
        public string Expiration { get; set; }
    }
}
