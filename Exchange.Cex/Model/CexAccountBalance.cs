using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Incryptex.MMS.Exchange.Cex.Model
{
    public class CexAccountBalance
    {
        public string Currency { get; set; }

        [JsonProperty(PropertyName = "available")]
        public decimal AvailableAmount { get; set; }

        public static List<CexAccountBalance> GetFromJObject(JObject o)
        {
            var resultList = new List<CexAccountBalance>();
            if (o != null)
            {
                resultList = o.OfType<JProperty>().Skip(2)
                            .Select(x => new CexAccountBalance
                            {
                                Currency = x.Name,
                                AvailableAmount = x.Value.First.ToObject<decimal>()
                            }).ToList();
            }
            return resultList;
        }
    }
}
