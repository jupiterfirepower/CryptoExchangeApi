using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Common.Contracts
{
    [Serializable]
    public class OrderBook
    {
        private OrderBook(OrderBook src)
        {
            Asks = src.Asks;
            Bids = src.Bids;
            Exchange = src.Exchange;
            Pair = src.Pair;
            Time = src.Time;
            IsStored = src.IsStored;
        }
        private OrderBook(string exchange, Pair pair)
        {
            Asks = new ConcurrentBag<Order>();
            Bids = new ConcurrentBag<Order>();
            Exchange = exchange;
            Pair = pair;
            IsStored = false;
            Time = DateTime.UtcNow;
        }
        [JsonConstructor]
        public OrderBook(IEnumerable<Order> bids, IEnumerable<Order> asks, string exchange, Pair pair, DateTime time)
        {
            asks = asks.OrderByDescending(ask => ask.Price); // order is intentionally reversed due to ConcurrentBag construction
            bids = bids.OrderBy(bid => bid.Price);
            Asks = new ConcurrentBag<Order>(asks);
            //asks.Map(Asks.Add);
            Bids = new ConcurrentBag<Order>(bids);
            //bids.Map(Bids.Add);
            Exchange = exchange;
            Pair = pair;
            Time = time;
            IsStored = false;
        }
        public OrderBook GetFixedTop(int numberOfRecords)
        {
            return new OrderBook(
                Bids.Take(numberOfRecords).ToList(),
                Asks.Take(numberOfRecords).ToList(),
                Exchange,
                Pair,
                Time)
            { IsStored = this.IsStored };
        }
        public OrderBook ApplySpread(decimal bidSpread, decimal askSpread)
        {
            return new OrderBook(
                Bids.Select(o => o.TransformPrice(bidSpread, 0)).ToList(),
                Asks.Select(o => o.TransformPrice(askSpread, 0)).ToList(),
                Exchange,
                Pair,
                Time)
            { IsStored = this.IsStored };
        }

        public OrderBook UpdatePair(Pair pair)
        {
            var bids = Bids.Select(o => o.UpdatePair(pair));
            var asks = Asks.Select(o => o.UpdatePair(pair));
            return new OrderBook(bids, asks, Exchange, pair, Time);
        }

        public DateTime Time { get; private set; }
        public ConcurrentBag<Order> Asks { get; private set; }
        public ConcurrentBag<Order> Bids { get; private set; }
        public string Exchange { get; private set; }
        public Pair Pair { get; private set; }
        public bool IsStored { get; private set; }
        public OrderBook SetIsStored()
        {
            return new OrderBook(this) { IsStored = true };
        }

        public bool Equals(OrderBook other)
        {

            bool equal = Asks.SequenceEqual(other.Asks) && Bids.SequenceEqual(other.Bids);
            // Trace.WriteLine("Equals {0} {1},{2}".FormatAs(equal,this,other));
            return equal;
            //return !(
            //    Asks.Except(other.Asks).Any() || 
            //    other.Asks.Except(Asks).Any() || 
            //    Bids.Except(other.Bids).Any() || 
            //    other.Bids.Except(Bids).Any());
        }
        public static IEqualityComparer<OrderBook> GetComparer()
        {
            return new OrderBookEqualityComparer();
        }

        public object Clone()
        {
            return new OrderBook(this);
        }

        public override string ToString()
        {
            return string.Format("ExchOB {0} {1} Asks({2}) {3:f4}@{4:f4} Bids({5}) {6:f4}@{7:f4}",
                Exchange,
                Pair,
                Asks.Count,
                Asks.Count == 0 ? 0 : Asks.First().Amount,
                Asks.Count == 0 ? 0 : Asks.First().Price,
                Bids.Count,
                Bids.Count == 0 ? 0 : Bids.First().Amount,
                Bids.Count == 0 ? 0 : Bids.First().Price);

        }

        public static OrderBook Empty(string exchange, Pair pair)
        {
            return new OrderBook(exchange, pair);
        }

        public OrderBook AddOrder(Order mmsOrder)
        {
            if (mmsOrder.MarketSide == MarketSide.Ask)
            {
                if (Asks.Any(askOrder => askOrder.MMSId == mmsOrder.MMSId))
                    throw new ApplicationException("Ask Order added received, but existing order found: {0}".FormatAs(mmsOrder));

                List<Order> newAsks = Asks.ToList();
                newAsks.Add(mmsOrder);

                return new OrderBook(Bids, newAsks, Exchange, Pair, Time);
            }
            else
            {
                if (Bids.Any(bidOrder => bidOrder.MMSId == mmsOrder.MMSId))
                    throw new ApplicationException("Bid Order added received, but existing order found: {0}".FormatAs(mmsOrder));

                List<Order> newBids = Bids.ToList();
                newBids.Add(mmsOrder);

                return new OrderBook(newBids, Asks, Exchange, Pair, Time);
            }
        }
        public OrderBook DeleteOrder(Order mmsOrder)
        {
            if (mmsOrder.MarketSide == MarketSide.Ask)
            {
                if (Asks.All(askOrder => askOrder.MMSId != mmsOrder.MMSId))
                    throw new ApplicationException("Ask Order deleted received, but no existing order found: {0}".FormatAs(mmsOrder));
                // remove 
                var newAsks = Asks.Where(o => o.MMSId != mmsOrder.MMSId).ToList();

                return new OrderBook(Bids, newAsks, Exchange, Pair, Time);
            }
            else
            {
                if (Bids.All(bidOrder => bidOrder.MMSId != mmsOrder.MMSId))
                    throw new ApplicationException("Bid Order deleted received, but no existing order found: {0}".FormatAs(mmsOrder));
                // remove 
                var newBids = Bids.Where(o => o.MMSId != mmsOrder.MMSId).ToList();

                return new OrderBook(newBids, Asks, Exchange, Pair, Time);
            }
        }

        public OrderBook UpdateOrder(Order mmsOrder)
        {
            if (mmsOrder.MarketSide == MarketSide.Ask)
            {
                if (Asks.All(askOrder => askOrder.MMSId != mmsOrder.MMSId))
                    throw new ApplicationException("Ask Order changed received, but no existing order found: {0}".FormatAs(mmsOrder));
                // remove 
                var newAsks = Asks.Where(o => o.MMSId != mmsOrder.MMSId).ToList();
                // add
                newAsks.Add(mmsOrder);
                return new OrderBook(Bids, newAsks, Exchange, Pair, Time);
            }
            else
            {
                if (Bids.All(bidOrder => bidOrder.MMSId != mmsOrder.MMSId))
                    throw new ApplicationException("Bid Order changed received, but no existing order found: {0}".FormatAs(mmsOrder));
                // remove 
                var newBids = Bids.Where(o => o.MMSId != mmsOrder.MMSId).ToList();
                // add
                newBids.Add(mmsOrder);
                return new OrderBook(newBids, Asks, Exchange, Pair, Time);
            }

        }

        public bool IsEmpty { get { return Asks.IsEmpty && Bids.IsEmpty; } }
    }
}
