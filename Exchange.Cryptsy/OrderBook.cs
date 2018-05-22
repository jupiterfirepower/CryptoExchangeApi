/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Exchange.Cryptsy
{
    public class CryptsyOrderBook
    {
        public List<CryptsyOrder> BuyOrders { get; private set; }
        public List<CryptsyOrder> SellOrders { get; private set; }

        public CryptsyOrderBook()
        {
            BuyOrders = new List<CryptsyOrder>();
            SellOrders = new List<CryptsyOrder>();
        }

        public CryptsyOrderBook(List<CryptsyOrder> buyOrders, List<CryptsyOrder> sellOrders)
        {
            BuyOrders = buyOrders;
            SellOrders = sellOrders;
        }

        public static CryptsyOrderBook ReadFromJObject(JObject o, Int64 marketId)
        {
            var ob = new CryptsyOrderBook();
            foreach (var order in o["sellorders"])
            {
                ob.SellOrders.Add(CryptsyOrder.ReadFromJObject(order as JObject, marketId, CryptsyOrder.ORDER_TYPE.SELL));
            }

            foreach (var order in o["buyorders"])
            {
                ob.BuyOrders.Add(CryptsyOrder.ReadFromJObject(order as JObject, marketId, CryptsyOrder.ORDER_TYPE.BUY));
            }

            return ob;
        }

        //returns: <marketID,OrderBook>
        public static Dictionary<Int64,CryptsyOrderBook> ReadMultipleFromJObject(JObject o)
        {
            var markets = new Dictionary<Int64,CryptsyOrderBook>();
            
            if (o == null)
                return markets;

            foreach (var market in o.Children())
            {
                var marketId = market.First().Value<Int64>("marketid");
                markets.Add(marketId,ReadFromJObject(market.First() as JObject, marketId));
            }

            return markets;
        }
    }
}
