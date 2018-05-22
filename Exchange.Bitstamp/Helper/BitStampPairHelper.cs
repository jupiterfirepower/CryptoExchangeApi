using System;
using Common.Contracts;

namespace Exchange.Bitstamp.Helper
{
    public class BitStampPairHelper
    {
        public static string ToString(Pair pair)
        {
            return String.Format("{0}_{1}", pair.BaseCurrency.ToString().ToLower(),
                pair.CounterCurrency.ToString().ToLower());
        }
    }
}
