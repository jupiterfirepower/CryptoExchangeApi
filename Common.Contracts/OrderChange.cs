using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Common.Contracts
{
    [Serializable]
    public class OrderChange : ISerializable
    {
        public OrderChange(Order order, decimal fillAmount, decimal avgPrice, OrderStatus changeType, DateTime time, decimal totalFilledAmount = 0, decimal fee = 0, decimal amount = 0)
        {
            Order = order;
            CurrentFillAmount = fillAmount;
            TotalFilledAmount = totalFilledAmount;
            CurrentAvgPrice = avgPrice;
            Exchange = order.Exchange;
            OrderStatus = changeType;
            Fee = fee;
            Value = amount;
            Time = time;
        }

        #region ISerializable

        protected OrderChange(SerializationInfo info, StreamingContext context)
        {

            if (info == null)
                throw new ArgumentNullException("info");
            Exchange = (string)info.GetValue("Exchange", typeof(string));
            Order = (Order)info.GetValue("Order", typeof(Order));
            Time = (DateTime)info.GetValue("Time", typeof(DateTime));
            CurrentAvgPrice = (decimal)info.GetValue("CurrentAvgPrice", typeof(decimal));
            CurrentAvgPrice = (decimal)info.GetValue("CurrentFillAmount", typeof(decimal));
            OrderStatus = (OrderStatus)info.GetValue("CurrentFillAmount", typeof(OrderStatus));
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info");
            info.AddValue("Exchange", Exchange);
            info.AddValue("Order", Order);
            info.AddValue("Time", Time);
            info.AddValue("CurrentAvgPrice", CurrentAvgPrice);
            info.AddValue("CurrentFillAmount", CurrentFillAmount);
            info.AddValue("OrderStatus", OrderStatus);
        }

        #endregion

        public string Exchange { get; private set; }
        public Order Order { get; private set; }
        public DateTime Time { get; private set; }
        public string TimeStr { get { return Time.ToString("O"); } }
        public decimal CurrentAvgPrice { get; private set; }
        public decimal CurrentFillAmount { get; private set; }
        public decimal TotalFilledAmount { get; private set; }
        public decimal Fee { get; private set; }
        public decimal Value { get; private set; }

        public OrderStatus OrderStatus { get; private set; }
        public override string ToString()
        {
            return ""; // "{0} {1} {2} Qty:{3:f4} @ Avg:{4:f4}".FormatAs(OrderStatus, Time, Order, CurrentFillAmount, CurrentAvgPrice);
        }
    }
}
