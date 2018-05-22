/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Exchange.Cryptsy.Enums;
using Newtonsoft.Json.Linq;

namespace Exchange.Cryptsy.Model
{
    public class CryptsyTransaction //Deposit or Withdrawal
    {
        public string Currency{get; private set;}
        public DateTime DateTimeUtc { get; private set; }
        public CryptsyTransactionType TransactionType { get; private set; }
        public String Address { get; private set; } //Address to which the deposit posted or Withdrawal was sent
        public decimal Amount { get; private set; } //Not including fees
        public decimal Fee { get; private set; }

        public static CryptsyTransaction ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;

            var t = new CryptsyTransaction
            {
                Currency = o.Value<string>("currency"),
                DateTimeUtc = TimeZoneInfo.ConvertTime(o.Value<DateTime>("datetime"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc),
                TransactionType = o.Value<string>("type").ToLower() == "deposit" ? CryptsyTransactionType.Deposit : CryptsyTransactionType.Withdrawal,
                Address = o.Value<string>("address"),
                Amount = o.Value<decimal>("amount"),
                Fee = o.Value<decimal>("fee")
            };

            return t;
        }

        public static List<CryptsyTransaction> ReadMultipleFromJArray(JArray array)
        {
            if (array == null)
                return new List<CryptsyTransaction>();

            return (from JObject o in array select ReadFromJObject(o)).ToList();
        }
    }
}
