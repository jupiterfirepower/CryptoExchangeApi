using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Coinfloor.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Incryptex.MMS.Exchange.Coinfloor
{
    public class CoinfloorApi: ICoinfloorApi
    {
        private readonly string _key;
        private readonly string _secret;
        private const string ApiUrl = "https://webapi.coinfloor.co.uk:8090/bist/";

        #region Logger
        #endregion
        public CoinfloorApi(string key, string secret)
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

        public async Task<CoinfloorTicker> GetTickers(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<CoinfloorTicker>(ApiUrl +
                                                                                      $"{pair.BaseCurrency}/{pair.CounterCurrency}/ticker/", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await Query<CoinfloorOrderBook>(ApiUrl +
                                                           $"{pair.BaseCurrency}/{pair.CounterCurrency}/order_book/", token);
                return FromCoinfloorOrderBook(data, pair);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<CoinfloorTransaction>> GetTransactions(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<List<CoinfloorTransaction>>(ApiUrl +
                                                                                                 $"{pair.BaseCurrency}/{pair.CounterCurrency}/transactions/", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #region Special Methods
        private OrderBook FromCoinfloorOrderBook(CoinfloorOrderBook orderBook, Pair pair)
        {
            const string exchangeName = ExchangeName.Coinfloor;
            var ob = new OrderBook(
                orderBook.Bids.Select(
                order => new Order
                    (
                        pair,
                        order[0],
                        order[1],
                        exchangeName,
                        MarketSide.Bid,
                        DateTime.UtcNow,
                        OrderType.Limit,
                        SourceSystemCode.ExternalExchange
                    )).ToList(),
                orderBook.Asks.Select(
                order => new Order
                    (
                        pair,
                        order[0],
                        order[1],
                        exchangeName,
                        MarketSide.Ask,
                        DateTime.UtcNow,
                        OrderType.Limit,
                        SourceSystemCode.ExternalExchange
                    )).ToList(),
                exchangeName,
                pair,
                DateTime.UtcNow);
            return ob;
        }
        #endregion
    }
}
