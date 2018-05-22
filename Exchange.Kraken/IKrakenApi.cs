using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Kraken.Model;

namespace Exchange.Kraken
{
    public interface IKrakenApi : IGetOrderStatus
    {
        bool IsRestartAfterTime { get; set; }
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken));
        Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken));
        Task<DateTime> GetTime(CancellationToken token = default(CancellationToken));
        Task<KrakenAssets> GetAssets(CancellationToken token = default(CancellationToken));
        Task<KrakenAssetPairs> GetAssetPairs(CancellationToken token = default(CancellationToken));
        Task<KrakenTicket> GetTicker(Pair pair, CancellationToken token = default(CancellationToken));
        Task<KrakenData> GetOhlc(Pair pair, CancellationToken token = default(CancellationToken));
        Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken));
        Task<KrakenData> GetTrades(Pair pair, CancellationToken token = default(CancellationToken));
        Task<KrakenData> GetSpread(Pair pair, CancellationToken token = default(CancellationToken));
        Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<int> CancelOrder(string txid, CancellationToken token = default(CancellationToken));
        Task<KrakenOrder> GetOrder(string txid, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<KrakenOrder>> GetOpenOrders(bool trades = false, string userref = "", CancellationToken token = default(CancellationToken));
    }
}
