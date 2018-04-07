using Exchange.Bter.Responses;
using Newtonsoft.Json.Linq;

namespace Exchange.Bter.Model
{
    public class BterPrivateResult
    {
        public BterResponse BterResponse { get; set; }
        public JObject JObject { get; set; }
    }
}
