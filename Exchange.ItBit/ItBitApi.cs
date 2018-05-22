using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.ItBit.Model;

namespace Incryptex.MMS.Exchange.ItBit
{
    public class ItBitApi : IItBitApi
    {
        private readonly string _key;
        private readonly string _secret;
        //private IItBitConfigLive _config;
        private const string ApiUrl = "https://www.itbit.com/api/v2/";
        private const string ApiUrlV1 = "https://api.itbit.com/v1/";

        #region Logger
        //protected readonly ILogger Log;
        #endregion
        public ItBitApi(string key, string secret)
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
            string resultData = String.Empty;
            try
            {
                resultData = await Query(url, token);
                return JsonConvert.DeserializeObject<T>(resultData);
            }
            catch (Exception)
            {
                //Log.Error("ItBit: Can't parse json {0} to ItBit<{1}>", resultData, typeof(T));
                throw;
            }
        }

        private async Task<string> QueryWithUserAgent(string url, CancellationToken token = default(CancellationToken))
        {
            return await QueryHelper.QueryWithUserAgent(url, token);
        }

        private async Task<T> QueryWithUserAgent<T>(string url, CancellationToken token = default(CancellationToken))
        {
            string resultData = String.Empty;
            try
            {
                resultData = await QueryWithUserAgent(url, token);
                return JsonConvert.DeserializeObject<T>(resultData);
            }
            catch (Exception)
            {
                //Log.Error("ItBit: Can't parse json {0} to ItBit<{1}>", resultData, typeof(T));
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
            string resultData = String.Empty;
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
                        //Log.Error("ItBit: Can't parse json {0} to ItBit<{1}>, URL - {2}, Exception Message - {3}, Nonce - {4}", resultData, typeof(T), url, ex.Message, parameters["nonce"]);
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

        public async Task<ItBitTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<ItBitTicker>(ApiUrlV1 + String.Format("markets/{0}{1}/ticker", pair.BaseCurrency, pair.CounterCurrency), token), 
                TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await QueryWithUserAgent<ItBitOrderBook>(ApiUrl + String.Format("markets/{0}{1}/orders", pair.BaseCurrency, pair.CounterCurrency), token);
                return FromItBitOrderBook(data, pair);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<ItBitTrade>> GetTrades(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await QueryWithUserAgent<List<ItBitTrade>>(ApiUrl + String.Format("markets/{0}{1}/trades?since=0", pair.BaseCurrency, pair.CounterCurrency), token),
                TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken))
        {
            return await Task<IEnumerable<Pair>>.Factory.StartNew(() =>
            {
                var list = new List<Pair>
                {
                    new Pair(SupportedCurrency.XBT, SupportedCurrency.USD),
                    new Pair(SupportedCurrency.XBT, SupportedCurrency.EUR),
                    new Pair(SupportedCurrency.XBT, SupportedCurrency.SGD),
                };
                return list.AsEnumerable();
            }, token);
        }

        #region Special Methods
        private OrderBook FromItBitOrderBook(ItBitOrderBook orderBook, Pair pair)
        {
            const string exchangeName = ExchangeName.ItBit;
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
