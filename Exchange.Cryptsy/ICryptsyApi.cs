using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Cryptsy.Enums;
using Exchange.Cryptsy.Model;
using Exchange.Cryptsy.Responses;


namespace Exchange.Cryptsy
{
    public interface ICryptsyApi : IGetOrderStatus
    {
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken));
        CryptsyMarket[] GetMarkets(CancellationToken token = default(CancellationToken));
        Task<CryptsyMarketInfo> GetMarketInfo(string currencyCode1, string currencyCode2, bool basicInfoOnly = false, CancellationToken token = default(CancellationToken));
        Task<CryptsyMarketInfo> GetMarketInfo(long marketId, CancellationToken token = default(CancellationToken));
        Task<ConcurrentBag<CryptsyMarketInfo>> GetOpenMarkets(bool basicInfoOnly = false, CancellationToken token = default(CancellationToken));
        Task<CryptsyOrderBook> GetOrderBook(long marketId, CancellationToken token = default(CancellationToken));

        Task<Dictionary<Int64, CryptsyOrderBook>> GetAllOrderBooks(CancellationToken token = default(CancellationToken));

        Task<CryptsyAccountBalance> GetBalance(CancellationToken token = default(CancellationToken));

        Task<decimal> CalculateFee(CryptsyOrderType orderType, decimal quantity, decimal price, CancellationToken token = default(CancellationToken));

        Task<string> GenerateNewAddress(string currencyCode, CancellationToken token = default(CancellationToken));

        Task<List<CryptsyTrade>> GetMarketTrades(long marketId, CancellationToken token = default(CancellationToken));

        Task<List<CryptsyTrade>> GetMyTrades(long marketId, uint limitResults = 200, CancellationToken token = default(CancellationToken));

        Task<List<CryptsyOrder>> GetMyOrders(long marketId, CancellationToken token = default(CancellationToken));

        Task<List<CryptsyOrder>> GetAllMyOrders(CancellationToken token = default(CancellationToken));

        Task<List<CryptsyTrade>> GetAllMyTrades(CancellationToken token = default(CancellationToken));

        Task<List<CryptsyTransaction>> GetTransactions(CancellationToken token = default(CancellationToken));

        Task<CryptsyOrderResult> CreateOrder(long marketId, CryptsyOrderType orderType, decimal quantity, decimal price, CancellationToken token = default(CancellationToken));

        Task<CryptsyOrderResult> CancelOrder(long orderId,CancellationToken token = default(CancellationToken));

        Task<List<string>> CancelAllMarketOrders(long marketId,CancellationToken token = default(CancellationToken));

        Task<List<string>> CancelAllOrders(CancellationToken token = default(CancellationToken));

        Task<string> BuyLimit(int marketId, Order order, CancellationToken token = default(CancellationToken));
        Task<string> SellLimit(int marketId, Order order, CancellationToken token = default(CancellationToken));

        ConcurrentBag<CryptsyMarketInfo> GetOpenMarketsPeriodically(CryptsyMarket[] markets, bool basicInfoOnly = false);
    }
}
