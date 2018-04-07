using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Bittrex.Model;

namespace Exchange.Bittrex
{
    public interface IBittrexApi: IGetOrderStatus
    {
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken));
        Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<bool> CancelOrder(string uuid, CancellationToken token = default(CancellationToken));
        Task<BittrexGetOrderRecord> GetOrder(string uuid, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<BittrexGetOpenOrdersRecord>> GetAllOpenOrders(CancellationToken token = default(CancellationToken));
        /// <summary>
        /// Unfortunately, Bittrex supports only first 50 records of the orderbook
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<BittrexMarket>> GetMarkets(CancellationToken token = default(CancellationToken));
        Task<IEnumerable<BittrexMarketSummary>> GetMarketSummaries(CancellationToken token = default(CancellationToken));
        Task<BittrexTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken));
    }
}