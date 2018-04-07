using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Response
{
    public class CexResponse
    {
        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }
    }
}
