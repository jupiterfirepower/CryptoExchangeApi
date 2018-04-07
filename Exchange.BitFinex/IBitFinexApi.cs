using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using BitFinex.Model;
using BitFinex.Responses;
using BitFinex.Enums;

namespace BitFinex
{
    public interface IBitFinexApi : IGetOrderStatus
    {
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken));
        Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken));

        Task<List<Pair>> GetPairs(CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitFinexPairDetails>> GetPairsDetails(CancellationToken token = default(CancellationToken));

        Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken));

        Task<BitFinexTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken));

        Task<BitFinexLandBook> GetLandBook(string currency, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitFinexTrade>> GetTrades(Pair pair, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitFinexLend>> GetLends(string currency, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitFinexStats>> GetStats(Pair pair, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitFinexOrderStatus>> GetActiveOrders(CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitFinexWalletBalance>> GetWalletBalances(CancellationToken token = default(CancellationToken));

        Task<List<BitFinexAccountInfo>> GetAccountInfos(CancellationToken token = default(CancellationToken));

        Task<List<BitFinexMargin>> GetMarginInfos(CancellationToken token = default(CancellationToken));

        Task<BitFinexOrderStatus> NewOrder(Order order, BitFinexOrderSide side, string orderType, CancellationToken token = default(CancellationToken));

        Task<BitFinexOrderStatus> CancelOrder(int uuid, CancellationToken token = default(CancellationToken));

        Task<BitFinexResponse> CancelAllOrder(CancellationToken token = default(CancellationToken));

        Task<BitFinexOrderStatus> GetOrderStatus(string uuid, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitFinixPosition>> GetPositions(CancellationToken token = default(CancellationToken));

        Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken));

        Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken));

        Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken));

        Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken));
    }
}
