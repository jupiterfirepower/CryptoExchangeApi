using System;
using System.Collections.Concurrent;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Exchange.Bter.Model
{
    public class BterAccountBalance
    {
        public string Currency { get; set; }

        public decimal AvailableAmount { get; set; }

        public decimal LockedAmount { get; set; }

        public static ConcurrentBag<BterAccountBalance> GetFromJObject(JObject o)
        {
            var resultList = new ConcurrentBag<BterAccountBalance>();
            if (o != null)
            {
                if (o["locked_funds"] != null)
                {
                    var locked = o["locked_funds"].OfType<JProperty>()
                            .Select(x =>new BterAccountBalance
                                    {
                                        Currency = x.Name,
                                        LockedAmount = x.Value.ToObject<decimal>()
                                    }).ToList();

                    Array.ForEach(o["available_funds"].OfType<JProperty>().Select(x =>
                    {
                        var lockedExists = locked.FirstOrDefault(m => m.Currency == x.Name);

                        var balance = new BterAccountBalance
                        {
                            Currency = x.Name,
                            AvailableAmount = x.Value.ToObject<decimal>(),
                            LockedAmount =
                                lockedExists != null
                                    ? lockedExists.LockedAmount
                                    : 0m
                        };

                        return balance;
                    }).ToArray(), resultList.Add);
                }
                else
                {
                    Array.ForEach(o["available_funds"].OfType<JProperty>().Select(x =>
                        new BterAccountBalance
                        {
                            Currency = x.Name,
                            AvailableAmount = x.Value.ToObject<decimal>(),
                        }).ToArray(), resultList.Add);
                }
            }

            return resultList;
        }
    }
}
