namespace BitFinex.Enums
{
    public enum BitFinexOrderSide
    {
        Sell,
        Buy
    }

    public class BitFinexOrderType
    {
        public const string Market = "market";
        public const string Limit = "limit";
        public const string Stop = "stop";
        public const string TrailingStop = "trailing-stop";
        public const string FillOrKill = "fill-or-kill";
        public const string ExchangeMarket = "exchange market";
        public const string ExchangeLimit = "exchange limit";
        public const string ExchangeStop = "exchange stop";
        public const string ExchangeTrailingStop = "exchange trailing-stop";
        public const string ExchangeFillOrKill = "exchange fill-or-kill";
    }
}
