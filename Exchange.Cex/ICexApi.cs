using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Cex.Model;
using Exchange.Cex.Responses;


namespace Exchange.Cex
{
    public interface ICexApi : IGetOrderStatus
    {
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<CexTicker> GetTickers(Pair pair, CancellationToken token = default(CancellationToken));
        Task<CexLastPrice> GetLastPrice(Pair pair, CancellationToken token = default(CancellationToken));
        Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<CexTradeHistory>> GetTradeHistory(Pair pair,CancellationToken token = default(CancellationToken));

        Task<CexBalance> GetBalances(CancellationToken token = default(CancellationToken));
        Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken));
        Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<CexOpenOrder>> GetOpenOrders(Pair pair, CancellationToken token = default(CancellationToken));
        Task<bool> CancelOrder(string orderId, CancellationToken token = default(CancellationToken));
        Task<CexPlaceOrderResponse> PlaceOrder(Pair pair, OrderSide type, decimal amount, decimal price, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<Pair>> GetSupportedPairs();
    }
}
