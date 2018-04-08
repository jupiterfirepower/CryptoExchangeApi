using Newtonsoft.Json;

namespace Exchange.Cex.Model
{
    public class CexLastPrice
    {
        [JsonProperty(PropertyName = "lprice")]
        public decimal LPrice { get; set; }
    }
}
