using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Contracts
{
    public interface IGetOrderStatus
    {
        Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken));
    }
}
