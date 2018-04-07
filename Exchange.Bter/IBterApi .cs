using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Bter.Enums;
using Exchange.Bter.Model;
using Exchange.Bter.Responses;

namespace Exchange.Bter
{
    public interface IBterApi : IGetOrderStatus
    {
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<ConcurrentBag<BterPairInfo>> GetPairs(CancellationToken token = default(CancellationToken));

        Task<ConcurrentBag<BterMarketInfo>> GetMarketInfo(CancellationToken token = default(CancellationToken));

        Task<ConcurrentBag<BterTicker>> GetTickers(CancellationToken token = default(CancellationToken));

        Task<BterTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken));

        Task<List<BterTradeHistory>> GetTradeHistory(Pair pair, CancellationToken token = default(CancellationToken));

        Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken));

        Task<string> PlaceOrder(Order order, BterOrderType type, CancellationToken token = default(CancellationToken));

        Task<BterOrderStatus> GetOrder(string orderId, CancellationToken token = default(CancellationToken));

        Task<ConcurrentBag<BterOrder>> GetOrderList(CancellationToken token = default(CancellationToken));

        Task<BterResponse> CancelOrder(string uuid, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken));

        Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken));

        Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken));
    }
}
