using System;

namespace Exchange.Bittrex.Model
{
    public class BittrexMarketSummary
    {
        public string MarketName{ get; set;}
        public decimal? High { get; set; }
        public decimal? Low { get; set; }
        public decimal? Volume { get; set; }
        public decimal? Last { get; set; }
        public decimal? BaseVolume { get; set; }
        public DateTime TimeStamp { get; set;}
        public decimal? Bid { get; set; }
        public decimal? Ask { get; set; }
        public int OpenBuyOrders{ get; set;}
        public int OpenSellOrders{ get; set;}
        public decimal? PrevDay { get; set; }
        public DateTime Created { get; set; }
        public string DisplayMarketName { get; set; }
        public override string ToString()
        {
            return string.Format("Market:{0} Last:{1} Volume:{2}", MarketName, Last, Volume);
        }
    }
}
