namespace Common.Contracts
{
    public static class Constant
    {
        public static readonly int SpinWaitOrSleep = 50;
        public static readonly int TimeOut = 10000;
        public static readonly int CryptsyTimeOut = 7000;
        public static int DefaultConnectionLimit = 50;
        public static int MaxIdleTime = 400;
        public static int MaxServicePoints = 30;
        public static int CryptsyRetryInterval = 1500;
        public static int CexRetryInterval = 1500;
        public static int DefaultRetryInterval = 1500;
        public static int AnxBtcRetryInterval = 1500;
        public static int KrakenRetryInterval = 1500;
        public static readonly int FillOrCancelInterval = 2000;
        public static readonly decimal PercentMinusFive = .95M;
        public static readonly decimal PercentPlusFive = .95M;
        public static readonly string OrderChange = "OrderChange";
        public static readonly string OrderBook = "OrderBook";
        public static readonly string ExchangeStatus = "ExchangeStatus";
        public static readonly string AccountChange = "AccountChange";
        public static readonly string ExchangeFailureException = "ExchangeFailureException";
        public static readonly string TaskGroupDownloadOrderBooks = "DownloadOrderBooks";
        public static readonly string TaskGroupAccountHoldings = "AccountHoldings";
    }
}
