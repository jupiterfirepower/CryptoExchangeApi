namespace Common.Contracts
{
    public enum RequestCategory
    {
        AccountHoldings,
        SubmitOrder,
        OrderStatus,
        OrderHistory,
        TransHistory,
        OpenOrders,
        CancelOrder,
        CancelAllOrder,
        Ignore // Special Category for ignore save json data 
    }
}
