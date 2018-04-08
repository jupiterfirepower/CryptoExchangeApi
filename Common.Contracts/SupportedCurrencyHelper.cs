using System.Linq;
using System.Reflection;

namespace Common.Contracts
{
    public static class SupportedCurrencyHelper
    {
        public static string[] GetSupportedCurrencies()
        {
            var supported = typeof(SupportedCurrency).GetFields(BindingFlags.Static | BindingFlags.Public).
                                 Select(item => item.GetValue(null)).
                                 Where(r => !r.Equals(SupportedCurrency.Unknown))
                                 .OfType<string>()
                                 .ToArray();
            return supported;
        }
    }
}
