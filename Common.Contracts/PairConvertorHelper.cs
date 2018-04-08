using System.Collections.Generic;
using System.Linq;

namespace Common.Contracts
{
    public class PairConvertorHelper
    {
        private const string Drk = "DRK";
        private const string Cny = "CNY";

        public static IEnumerable<Pair> GetDashToDrkPairPairs(IEnumerable<Pair> pairs)
        {
            return pairs.Select(DashToDrkPair).ToArray();
        }

        public static IEnumerable<Pair> GetDrkToDashPairPairs(IEnumerable<Pair> pairs)
        {
            return pairs.Select(DrkToDashPair).ToArray();
        }

        public static IEnumerable<Pair> GetCnhToCnyPairs(IEnumerable<Pair> pairs)
        {
            return pairs.Select(CnhToCnyPair).ToArray();
        }

        public static IEnumerable<Pair> GetCnyToCnhPairs(IEnumerable<Pair> pairs)
        {
            return pairs.Select(CnyToCnhPair).ToArray();
        }

        public static Pair DashToDrkPair(Pair pair)
        {
            return new Pair(
                pair.BaseCurrency == SupportedCurrency.DASH ?
                Drk : pair.BaseCurrency,
                pair.CounterCurrency == SupportedCurrency.DASH ?
                Drk : pair.CounterCurrency);
        }

        public static Pair DrkToDashPair(Pair pair)
        {
            return new Pair(
                pair.BaseCurrency == Drk ?
                SupportedCurrency.DASH : pair.BaseCurrency,
                pair.CounterCurrency == Drk ?
                 SupportedCurrency.DASH : pair.CounterCurrency);
        }

        public static Pair CnyToCnhPair(Pair pair)
        {
            return new Pair(
                pair.BaseCurrency == Cny ?
                SupportedCurrency.CNH : pair.BaseCurrency,
                pair.CounterCurrency == Cny ?
                 SupportedCurrency.CNH : pair.CounterCurrency);
        }

        public static Pair CnhToCnyPair(Pair pair)
        {
            return new Pair(
                pair.BaseCurrency == SupportedCurrency.CNH ?
                Cny : pair.BaseCurrency,
                pair.CounterCurrency == SupportedCurrency.CNH ?
                Cny : pair.CounterCurrency);
        }

        public static IEnumerable<OrderBook> GetCorrectOrderBooks(IEnumerable<OrderBook> orderBooks)
        {
            return orderBooks.Select(x => x.UpdatePair(DrkToDashPair(x.Pair))).ToArray();
        }

        public static IEnumerable<OrderBook> GetBtcChinaCorrectOrderBooks(IEnumerable<OrderBook> orderBooks)
        {
            return orderBooks.Select(x => x.UpdatePair(CnyToCnhPair(x.Pair))).ToList();
        }

        private static string GetCorrectCurrency(string currency)
        {
            return currency == Drk ? SupportedCurrency.DASH : currency;
        }

        public static string GetCnhCorrectCurrency(string currency)
        {
            return currency == Cny ? SupportedCurrency.CNH : currency;
        }

        public static IEnumerable<AccountChange> DrkToDashAccountHoldings(IEnumerable<AccountChange> accountChanges)
        {
            return accountChanges.Select(ac => new AccountChange(ac.ExchangeName, GetCorrectCurrency(ac.Currency), ac.Amount));
        }

        public static IEnumerable<AccountChange> CnyToCnhAccountHoldings(IEnumerable<AccountChange> accountChanges)
        {
            return accountChanges.Select(ac => new AccountChange(ac.ExchangeName, GetCnhCorrectCurrency(ac.Currency), ac.Amount));
        }
    }
}
