using Common.Contracts;

namespace Exchange.Bitstamp.Model
{
    public class BitStampExchangeRate
    {
        public Pair Pair { get; set; }

        public decimal Sell { get; set; }

        public decimal Buy { get; set; }
        
    }
}
