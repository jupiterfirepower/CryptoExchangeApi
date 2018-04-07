using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexLastPrice
    {
        [JsonProperty(PropertyName = "lprice")]
        public decimal LPrice { get; set; }
    }
}
