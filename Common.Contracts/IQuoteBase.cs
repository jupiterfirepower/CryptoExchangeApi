using System;

namespace Common.Contracts
{
    public interface IQuoteBase
    {
        MarketSide MarketSide { get; }
        decimal Price { get; }
        decimal Amount { get; }
        DateTime Time { get; }
        string Exchange { get; }
    }
}
