using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.RockTrading.Model;
using Exchange.RockTrading.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Exchange.RockTrading
{
    public class RockTradingApi : IRockTradingApi
    {
        private readonly string _key;
        private readonly string _secret;
        //private ICexConfigLive _config;
        private const string ApiUrl = "https://www.therocktrading.com/api/";

        #region Logger
        #endregion

        public RockTradingApi(string key, string secret)
        {
            _key = key;
            _secret = secret;
        }

        #region Public Query Methods
        private async Task<string> Query(string url, CancellationToken token = default(CancellationToken))
        {
            return await QueryHelper.Query(url, token);
        }

        private async Task<T> Query<T>(string url, CancellationToken token = default(CancellationToken))
        {
            string resultData = string.Empty;
            try
            {
                resultData = await Query(url, token);
                return JsonConvert.DeserializeObject<T>(resultData);
            }
            catch (Exception)
            {
                //Log.Error("Poloniex: Can't parse json {0} to Poloniex<{1}>", resultData, typeof(T));
                throw;
            }
        }

        private async Task<JArray> JArrayQuery(string url, CancellationToken token = default(CancellationToken))
        {
            var data = await Query(url, token);
            return JArray.Parse(data);
        }

        public async Task<JObject> JObjectQuery(string uri, CancellationToken token = default(CancellationToken))
        {
            var data = await Query(uri, token);
            return JObject.Parse(data);
        }
        #endregion

        #region Private Query Methods
        public async Task<T> PrivateQuery<T>(string url, Dictionary<string, string> parameters, CancellationToken token = default(CancellationToken))
        {
            string resultData = string.Empty;
            return await Task.Run(() =>
            {
                return RetryHelper.Do(() =>
                {
                    try
                    {
                        resultData = PrivateQuery(url, parameters, token).Result;

                        return JsonConvert.DeserializeObject<T>(resultData);
                    }
                    catch (Exception ex)
                    {
                        //Log.Error("Cex: Can't parse json {0} to Cex<{1}>, URL - {2}, Exception Message - {3}, Nonce - {4}", resultData, typeof(T), url, ex.Message, parameters["nonce"]);
                        throw;
                    }
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }, token);
        }

        private async Task<string> PrivateQuery(string url, Dictionary<string, string> parameters, CancellationToken token = default(CancellationToken))
        {
            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                using (HttpResponseMessage response = await client.PostAsync(new Uri(url), new FormUrlEncodedContent(parameters), token))
                {
                    // Read response asynchronously 
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }
        #endregion

        public async Task<TickerResponse> GetTicker(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<TickerResponse>(ApiUrl +
                                                                                     $"ticker/{pair.BaseCurrency}{pair.CounterCurrency}", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await Query<RockTradingOrderBook>(ApiUrl +
                                                             $"orderbook/{pair.BaseCurrency}{pair.CounterCurrency}", token);
                return FromRockTradingOrderBook(data, pair);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<RockTradingTrade>> GetTrades(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<List<RockTradingTrade>>(ApiUrl +
                                                                                             $"trades/{pair.BaseCurrency}{pair.CounterCurrency}", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #region Special Methods
        private OrderBook FromRockTradingOrderBook(RockTradingOrderBook orderBook, Pair pair)
        {
            const string exchangeName = ExchangeName.RockTrading;
            var ob = new OrderBook(
                orderBook.Bids.Where(x => x[0] > 0 && x[1] > 0).Select(b => FromRockTradingOrder(exchangeName, pair, MarketSide.Bid, b[0], b[1])),
                orderBook.Asks.Where(x => x[0] > 0 && x[1] > 0).Select(b => FromRockTradingOrder(exchangeName, pair, MarketSide.Ask, b[0], b[1])),
                exchangeName,
                pair,
                DateTime.UtcNow);
            return ob;
        }

        public Order FromRockTradingOrder(string exchangeName, Pair pair, MarketSide marketSide, decimal price, decimal amount)
        {
            return new Order(
                pair,
                price,
                amount,
                exchangeName,
                marketSide,
                DateTime.UtcNow,
                OrderType.Limit,
                SourceSystemCode.ExternalExchange);
        }
        #endregion
    }
}
