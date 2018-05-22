using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.Cryptsy.Model
{
    public class CryptsyAccountBalance
    {
        //<currency_code,available_balance>
        public Dictionary<string, decimal> BalanceAvailable { get; private set; }
        //<currency_code,balance_on_hold>
        public Dictionary<string, decimal> BalanceOnHold { get; private set; }
        public DateTime LastUpdateUtc { get; private set; }
        public Int32 OpenOrderCount { get; private set; }

        public static CryptsyAccountBalance ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;

            var bal = new CryptsyAccountBalance
            {
                BalanceAvailable =
                    JsonConvert.DeserializeObject<Dictionary<string, decimal>>(o["balances_available"].ToString()),
                BalanceOnHold = o["balances_hold"] == null ? null:
                    JsonConvert.DeserializeObject<Dictionary<string, decimal>>(o["balances_hold"].ToString()),
                LastUpdateUtc =
                    TimeZoneInfo.ConvertTime(o.Value<DateTime>("serverdatetime"),
                        TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc),
                OpenOrderCount = o.Value<Int32>("openordercount")
            };

            return bal;
        }

        public decimal GetCurrencyBalance(string currencyCode)
        {
            return BalanceAvailable[currencyCode.ToUpper()];
        }

    }
}
