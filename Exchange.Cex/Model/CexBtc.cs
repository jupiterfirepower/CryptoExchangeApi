using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexBtc
    {
        [JsonProperty(PropertyName = "available")]
        public decimal Available { get; set; }
    }
}
