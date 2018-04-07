namespace Exchange.Bittrex.Model
{
    internal class BittrexAccountBalanceRecord
    {
        public string Uuid { get; set; }
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public decimal Available { get; set; }
        public decimal Pending { get; set; }
        public string CryptoAddress { get; set; }
        public bool Requested { get; set; }
    }
}
