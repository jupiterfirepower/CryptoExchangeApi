using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.HitBtc.Model;
using Exchange.HitBtc.Responses;


namespace Exchange.HitBtc
{
    public interface IHitBtcApi : IGetOrderStatus
    {
        Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken));
        Task<IEnumerable<HitBtcTicker>> GetTickers(CancellationToken token = default(CancellationToken));

        Task<HitBtcTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken));

        Task<TradingBalanceResponse> GetTradingBalances(CancellationToken token = default(CancellationToken));

        Task<string> GetActiveOrders(CancellationToken token = default(CancellationToken));

        Task<string> GetPaymentBalances(CancellationToken token = default(CancellationToken));
    }
}
