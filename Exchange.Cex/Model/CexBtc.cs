using Newtonsoft.Json;

namespace Exchange.Cex.Model
{
    public class CexBtc
    {
        [JsonProperty(PropertyName = "available")]
        public decimal Available { get; set; }
    }
}
