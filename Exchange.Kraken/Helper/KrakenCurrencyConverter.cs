using System;
using Exchange.Kraken.Model;

namespace Exchange.Kraken.Helper
{
    public static class KrakenCurrencyConverter
    {
        public static string ConvertToString(this KrakenPair value)
        {
            return Enum.GetName(value.GetType(), value);
        }
    }
}
