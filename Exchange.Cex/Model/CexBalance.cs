using System.Collections.Generic;
using Newtonsoft.Json;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexBalance
    {
        [JsonProperty(PropertyName = "timestamp")]
        public string Timestamp { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        public List<CexAccountBalance> AccountBalances { get; set; }
    }
}
