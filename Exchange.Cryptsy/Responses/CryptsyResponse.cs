/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using System;
using Newtonsoft.Json.Linq;

namespace Exchange.Cryptsy.Responses
{
    public class CryptsyResponse
    {
        public bool Success { get; private set; }
        public JToken Data { get; private set; }
        public string Error { get; private set; }

        //Only sometimes available:
        public Int64 OrderId { get; private set; }
        public string Info { get; private set; }

        public static CryptsyResponse ReadFromJObject(JObject o)
        {
            var r = new CryptsyResponse
            {
                Success = o.Value<int>("success") == 1,
                Error = o.Value<string>("error"),
                Data = o.Value<JToken>("return"),
                OrderId = o.Value<Int64?>("orderid") ?? -1
            };

            r.Info = o.Value<string>("moreinfo") ?? r.Error;
            return r;
        }
    }
}
