using System;
using System.Linq;

namespace Common.Contracts
{
    [Serializable]
    public class Pair : IEquatable<Pair>
    {
        public Pair(string baseCurrency, string counterCurrency)
        {
            BaseCurrency = baseCurrency.ToUpper();
            CounterCurrency = counterCurrency.ToUpper();
        }

        public string BaseCurrency
        {
            get;
            private set;
        }

        public string CounterCurrency
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", BaseCurrency, CounterCurrency);
        }

        public override int GetHashCode()
        {
            return BaseCurrency.GetHashCode() ^ CounterCurrency.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var val = obj as Pair;
            if (val == null) return false;
            return val.Equals(this);
        }

        public bool Equals(Pair p)
        {
            return BaseCurrency.Equals(p.BaseCurrency) && CounterCurrency.Equals(p.CounterCurrency);
        }

        public Pair Clone()
        {
            return new Pair(BaseCurrency, CounterCurrency);
        }

        public static Pair GetPair(string baseC, string counterC)
        {
            return new Pair(baseC.ToUpper(), counterC.ToUpper());
        }

        public static Pair GetPair(string symbol)
        {
            if (symbol == null) return new Pair(SupportedCurrency.Unknown, SupportedCurrency.Unknown);
            var parts = symbol.Split('/', ':', '_', '-');
            if (parts.Count() == 2)
                return new Pair(parts[0].ToUpper(), parts[1].ToUpper());
            if (symbol.Length == 6)
                return new Pair(symbol.Substring(0, 3).ToUpper(), symbol.Substring(3, 3).ToUpper());

            return new Pair(SupportedCurrency.Unknown, SupportedCurrency.Unknown);
        }

        public static bool operator ==(Pair a, Pair b)
        {
            if ((ReferenceEquals(a, null) && ReferenceEquals(b, null)) || ReferenceEquals(a, b)) return true;
            if (ReferenceEquals(a, null) || ReferenceEquals(b, null)) return false;
            return a.BaseCurrency == b.BaseCurrency && a.CounterCurrency == b.CounterCurrency;
        }

        public static bool operator !=(Pair a, Pair b)
        {
            return !(a == b);
        }

        public static Pair Undefined { get { return new Pair(SupportedCurrency.Unknown, SupportedCurrency.Unknown); } }
    }
}
