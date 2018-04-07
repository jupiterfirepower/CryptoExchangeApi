using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
//using Incryptex.Common;
using Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Bter.Model
{
    public class BterPairInfo
    {
        public Pair Pair { get; set; }

        private string _pairs;

        public string Pairs
        {
            get { return _pairs;  }
            set
            {
                _pairs = value;
                GetPair(value);
            }
        }

        [JsonProperty("decimal_places")]
        public int DecimalPlaces { get; set; }

        [JsonProperty("min_amount")]
        public float MinAmount { get; set; }

        [JsonProperty("fee")]
        public float Fee { get; set; }

        public static ConcurrentBag<BterPairInfo> GetFromJObject(JObject o)
        {
            var resultList = new ConcurrentBag<BterPairInfo>();

            Array.ForEach(o.Last.First.Children<JObject>().Select(x =>
            {
                var propertyName = x.Properties().First().Name;
                var current = (x[propertyName]).ToObject<BterPairInfo>();
                current.Pairs = propertyName;
                return current;
            }).ToArray(),resultList.Add);

            return resultList;
        }

        private void GetPair(string value)
        {
            var data = value.Split('_');

            if (data.Count() > 1)
            {
                var supported = SupportedCurrencyHelper.GetSupportedCurrencies();

                var inCurrency = supported.FirstOrDefault(curency => curency == data[0].ToUpper());
                var outCurrency = supported.FirstOrDefault(curency => curency == data[1].ToUpper());

                if (inCurrency != null && outCurrency != null)
                {
                    Pair = new Pair(inCurrency, outCurrency);
                }
            }
        }
    }
}

