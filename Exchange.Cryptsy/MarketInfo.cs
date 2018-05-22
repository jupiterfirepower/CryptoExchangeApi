/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Exchange.Cryptsy
{
    public class MarketInfo
    {
        public string PrimaryCurrencyCode { get;  set; }
        public string PrimaryCurrencyName { get; set; }
        public string SecondaryCurrencyCode { get; set; }
        public string SecondaryCurrencyName { get; set; }
        public string Label { get; set; } //Name of the market, for example AMC,BTC
        public Int64 MarketID { get; set; }
        public decimal Volume { get; set; }
        public Trade LastTrade { get; set; }
        public List<Trade> RecentTrades { get;  set; } //null if MatketInfo was loaded with basicInfoOnly = true

        public CryptsyOrderBook OrderBook { get; set; } /* Can contain: - null if MarketInfo was loaded with basicInfoOnly = true
                                                          *              - top 20 buy & sell orders, if the marketinfo was loaded with basicInfoOnly = false (or not present)
                                                          *              - full orderbook, if method LoadFullOrderBook was called
                                                          */


        //If basicInfoOnly flag is set to true, RecentTrades & OrderBook (top 20) won't be loaded
        //This can be used to reduce unnecessary memory usage
        public static List<MarketInfo> ReadMultipleFromJObject(JObject o, bool basicInfoOnly = false)
        {
            List<MarketInfo> markets = new List<MarketInfo>();
            foreach (var market in o["markets"])
            {
                markets.Add(ReadFromJObject(market.First() as JObject, basicInfoOnly));
            }
            return markets;
        }

        //Loads the full order book of Cryptsy, instead of top 20 orders
        public async Task<CryptsyOrderBook> GetFullOrderBook(CryptsyApi api)
        {
            OrderBook = await api.GetOrderBook(MarketID);
            return OrderBook;
        }

        //If basicInfoOnly flag is set to true, RecentTrades & OrderBook (top 20) won't be loaded
        //This can be used to reduce unnecessary memory usage
        private static MarketInfo ReadFromJObject(JObject o, bool basicInfoOnly = false)
        {
            MarketInfo marketInfo = new MarketInfo()
            {
                MarketID = o.Value<Int64>("marketid"),
                Label = o.Value<string>("label"),
                PrimaryCurrencyCode = o.Value<string>("primarycode"),
                PrimaryCurrencyName = o.Value<string>("primaryname"),
                SecondaryCurrencyCode = o.Value<string>("secondarycode"),
                SecondaryCurrencyName = o.Value<string>("secondaryname"),
                Volume = o.Value<decimal>("volume"),
                //CreationTimeUTC = TimeZoneInfo.ConvertTime(o.Value<DateTime>("created"), TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"), TimeZoneInfo.Utc)
            };

            if (!basicInfoOnly)
            {

                marketInfo.RecentTrades = new List<Trade>();
                marketInfo.OrderBook = new CryptsyOrderBook();

                foreach (var t in o["recenttrades"])
                {
                    Trade trade = Trade.ReadFromJObject(t as JObject);
                    marketInfo.RecentTrades.Add(trade);
                    marketInfo.LastTrade = trade;
                }

                //orderbook is returnd as array of markets (with only the requested market)
                marketInfo.OrderBook = CryptsyOrderBook.ReadFromJObject(o,marketInfo.MarketID);

            }

            return marketInfo;
        }

    }
}
