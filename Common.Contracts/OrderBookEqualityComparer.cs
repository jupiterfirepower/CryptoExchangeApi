using System;
using System.Collections.Generic;

namespace Common.Contracts
{
    public class OrderBookEqualityComparer : IEqualityComparer<OrderBook>
    {
        public bool Equals(OrderBook x, OrderBook y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(OrderBook obj)
        {
            throw new NotImplementedException();
        }
    }
}
