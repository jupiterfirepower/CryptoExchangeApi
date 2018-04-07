namespace Exchange.Bittrex.Model
{
    internal class BittrexCurrency
    {
        public string Currency { get; set; }
        public string CurrencyLong { get; set; }
        public int MinConfirmation { get; set; }
        public decimal TxFee { get; set; }
        public bool IsActive{ get; set; }
        public string CoinType { get; set; }
        public string BaseAddress { get; set; }
    }
}
