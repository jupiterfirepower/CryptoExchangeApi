using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Bitstamp.Model;

namespace Exchange.Bitstamp
{
    public interface IBitStampApi : IGetOrderStatus
    {
        Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken));
        Task<BitStampTick> GetTick(CancellationToken token = default(CancellationToken));

        Task<BitStampOrderBook> GetOrderBook(CancellationToken token = default(CancellationToken));

        Task<List<BitStampTransact>> GetTransactions(int delta = 3600, CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitStampUserTransaction>> GetUserTransactions(int skip = 0, int limit = 20, string sort = "desc", CancellationToken token = default(CancellationToken));

        Task<BitStampExchangeRate> GetExchangeRate(Pair pair, CancellationToken token = default(CancellationToken));

        Task<Dictionary<string, string>> GetBalance(CancellationToken token = default(CancellationToken));

        Task<IEnumerable<BitStampOrder>> GetUserOpenOrders(CancellationToken token = default(CancellationToken));

        Task<bool> CancelOrder(string id, CancellationToken token = default(CancellationToken));

        Task<BitStampOrder> BuyLimit(decimal amount, decimal price, CancellationToken token = default(CancellationToken));

        Task<BitStampOrder> SellLimit(decimal amount, decimal price, CancellationToken token = default(CancellationToken));

    }
}
