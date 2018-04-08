using System.Collections.Generic;

namespace Common.Contracts
{
    public class ExchangeName
    {
        public const string Undefined = "Undefined";
        public const string InCryptEx = "InCryptEx";
        public const string BitStamp = "BitStamp";
        public const string Btce = "Btce";
        public const string Cryptsy = "Cryptsy";
        public const string IB = "IB";
        public const string AnxBtc = "AnxBtc";
        public const string Bittrex = "Bittrex";
        public const string Kraken = "Kraken";
        public const string Bter = "Bter";
        public const string BitFinex = "BitFinex";
        public const string Poloniex = "Poloniex";
        public const string Cex = "Cex";
        public const string Coinfloor = "Coinfloor";
        public const string RockTrading = "RockTrading";
        public const string ItBit = "ItBit";
        public const string BtcChina = "BtcChina";

        public static List<string> ToList()
        {
            return new List<string>
            {
                Undefined,
                InCryptEx,
                BitStamp,
                Btce,
                Cryptsy,
                IB,
                AnxBtc,
                Bittrex,
                Kraken,
                Bter,
                BitFinex,
                Poloniex,
                Cex,
                Coinfloor,
                RockTrading,
                ItBit,
                BtcChina
            };
        }
    }
}
