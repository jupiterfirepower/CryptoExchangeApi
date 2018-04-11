using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Poloniex.Model;
using Exchange.Poloniex.Responses;

namespace Exchange.Poloniex
{
    public interface IPoloniexApi : IGetOrderStatus
    {
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<PoloniexTicker>> GetTickers(CancellationToken token = default(CancellationToken));
        Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken));
        
        Task<IEnumerable<PoloniexCurrency>> GetCurrencies(CancellationToken token = default(CancellationToken));
        Task<GetVolumeResponse> GetVolume(Pair pair, CancellationToken token = default(CancellationToken));
        Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken));
        Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken));

        Task<IEnumerable<PoloniexBalance>> GetBalances(CancellationToken token = default(CancellationToken));
        Task<IEnumerable<PoloniexCompleteBalance>> GetCompleteBalances(CancellationToken token = default(CancellationToken));
        Task<CancelOrderResponse> CancelOrder(string orderNumber, Pair pair,CancellationToken token = default(CancellationToken));
        Task<CreateOrderResponse> NewOrder(Order order, OrderSide orderSide, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<PoloniexOpenOrder>> GetOpenOrders(Pair pair, CancellationToken token = default(CancellationToken));
        Task<IEnumerable<PoloniexOrderHistory>> GetTradeHistory(Pair pair, CancellationToken token = default(CancellationToken));
    }
}
