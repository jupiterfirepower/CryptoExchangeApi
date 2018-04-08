using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Common.Contracts
{
    [Serializable]
    public class Order : IComparable<Order>, IEquatable<Order>, IComparable, IComparer<IQuoteBase>, IQuoteBase
    {
        [JsonConstructor]
        public Order(Pair pair, decimal price, decimal amount, string exchange, MarketSide marketSide, DateTime time, OrderType orderType, string sourceSystemCode, string id = null, string mmsId = null, string parentId = null, TimeInForce timeInForce = TimeInForce.GoodTillCancel)
        {
            if (price <= 0 && orderType != OrderType.Pegged)
                throw new ArgumentException(string.Format("Can't create order, price can't be zero or below. Order: {0} {1} {2} {3}@{4}",
                    exchange, pair, marketSide, amount, price));

            //if (amount <= 0)
            //    throw new ArgumentException("Can't create order, amount can't be zero or below");
            //Todo temporary fix
            if (amount < 0)
                throw new ArgumentException(string.Format("Can't create order, amount can't be zero or below. Order: {0} {1} {2} {3}@{4}",
                    exchange, pair, marketSide, amount, price));

            Id = id;
            Price = price;
            Amount = amount;
            Exchange = exchange;
            MarketSide = marketSide;
            Time = time;
            Pair = pair;
            OrderType = orderType;
            SourceSystemCode = sourceSystemCode;
            MMSId = mmsId;
            ParentId = parentId;
            Underlying = new List<Order>();
            TimeInForce = timeInForce;
        }

        private Order(Order order)
        {
            Amount = order.Amount;
            Exchange = order.Exchange;
            MarketSide = order.MarketSide;
            Pair = order.Pair;
            Price = order.Price;
            Time = order.Time;
            Id = order.Id;
            MMSId = order.MMSId;
            ParentId = order.ParentId;
            OrderType = order.OrderType;
            SourceSystemCode = order.SourceSystemCode;
            Underlying = order.Underlying;
        }

        public int Compare(IQuoteBase x, IQuoteBase y)
        {
            return x.Price.CompareTo(y.Price);
        }

        public DateTime Time { get; private set; }
        public OrderType OrderType { get; private set; }
        public TimeInForce TimeInForce { get; private set; }
        public OrderStatus? OrderStatus { get; set; }
        public string TimeStr { get { return Time.ToString("O"); } }

        public List<Order> Underlying { get; set; }
        public decimal Price { get; private set; }

        public Pair Pair { get; private set; }
        public decimal Amount { get; private set; }

        public string Id { get; private set; }
        public string MMSId { get; private set; }
        public string ParentId { get; set; }
        public Order TransformPrice(decimal coefficient, decimal fix)
        {
            return new Order(this) { Price = Price + Price * coefficient + fix };
        }
        public string Exchange { get; private set; }
        public string SourceSystemCode { get; private set; }
        public MarketSide MarketSide { get; private set; }
        public decimal TotalPrice { get { return Amount * Price; } }

        public int CompareTo(Order other)
        {
            if (Equals(other))
                return 0;
            return -1;
        }
        public bool Equals(Order other)
        {
            if (
                (!string.IsNullOrEmpty(MMSId) && (MMSId == other.MMSId)) ||
                (!string.IsNullOrEmpty(Id) && (Id == other.Id)) ||
                string.IsNullOrEmpty(MMSId) &&
                Price.AlmostEquals(other.Price) &&
                Amount.AlmostEquals(other.Amount) &&
                MarketSide == other.MarketSide)
                return true;
            return false;
        }

        public override string ToString()
        {
            return "{0} {1} {2} {3} {4} {5:f4} @ {6:f4} Id:{7} MMSId: {8}".FormatAs(Time, Exchange, Pair, OrderType, MarketSide, Amount, Price, Id, MMSId);
        }

        public int CompareTo(object obj)
        {
            return Compare(this, (Order)obj);
        }

        public Order UpdateOrderId(string orderId)
        {
            return new Order(this) { Id = orderId };
        }
        public Order UpdateAmount(decimal newAmount)
        {
            return new Order(this) { Amount = newAmount };
        }

        public static string CreateNewMMSOrderId(string exchangeName)
        {
            return string.Format("{0}{1}{2}",
                exchangeName,
                DateTime.UtcNow.ToString("s"),
                Guid.NewGuid());
        }
        public Order UpdatePrice(decimal price)
        {
            return new Order(this) { Price = price };
        }

        public static string ListToString(IEnumerable<Order> orders)
        {
            return orders.Aggregate("", (current, order) => current + "\r\n" + order);
        }

        public decimal Liquidity { get { return MarketSide == MarketSide.Ask ? Amount : TotalPrice; } }

        public Order ToMarketOrder()
        {
            return new Order(this) { OrderType = OrderType.Market };
        }

        public Order UpdatePair(Pair pair)
        {
            return new Order(this) { Pair = pair };
        }
    }
}
