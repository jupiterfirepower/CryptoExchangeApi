using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.HitBtc.Enums;
using Exchange.HitBtc.Model;
using Exchange.HitBtc.Responses;

namespace Exchange.HitBtc
{
    public class HitBtcApi : IHitBtcApi
    {
        private readonly string _apiKey;
        private readonly string _secretKey;
        private readonly Uri _baseUri = new Uri("https://api.hitbtc.com/");
        private const string Version = "1";

        #region Logger
        #endregion

        public HitBtcApi(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _secretKey = apiSecret;
        }

        public async Task<TradingBalanceResponse> GetTradingBalances(CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();
            return await RetryHelper.DoAsync(async () => await PrivateQuery<TradingBalanceResponse>(
                $"api/{Version}/trading/balance", new Dictionary<string, string>
            {
                { "nonce", Convert.ToString(GetNonce(), culture) },
                { "apikey", _apiKey }
            }, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> GetPaymentBalances(CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();
            return await RetryHelper.DoAsync(async () => await PrivateQuery<string>($"api/{Version}/payment/balance", new Dictionary<string, string>
            {
                { "nonce", Convert.ToString(GetNonce(), culture) },
                { "apikey", _apiKey }
            }, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        // returns all orders in status new or partiallyFilled.
        public async Task<string> GetActiveOrders(CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();
            return await RetryHelper.DoAsync(async () => await PrivateQuery<string>(
                $"api/{Version}/trading/orders/active", new Dictionary<string, string>
            {
                { "nonce", Convert.ToString(GetNonce(), culture) },
                { "apikey", _apiKey }
            }, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> CancelOrder(string clientOrderId, string cancelRequestClientOrderId, Pair pair, HitBtcTradeType type, CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();
            return await RetryHelper.DoAsync(async () => await PrivateQuery<string>(
                $"api/{Version}/trading/cancel_order", new Dictionary<string, string>
            {
                { "nonce", Convert.ToString(GetNonce(), culture) },
                { "apikey", _apiKey },
                { "clientOrderId", clientOrderId },
                { "cancelRequestClientOrderId", cancelRequestClientOrderId },
                { "symbol", pair.ToString().Replace("/", string.Empty) },
                { "side", type.ToString() }
            }, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> GetRecentOrders(CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();
            return await RetryHelper.DoAsync(async () => await PrivateQuery<string>(
                $"api/{Version}/trading/orders/recent", new Dictionary<string, string>
            {
                { "nonce", Convert.ToString(GetNonce(), culture) },
                { "apikey", _apiKey },
                { "max_results", "1000" }
            }, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> NewOrder(string clientOrderId, Pair pair, HitBtcTradeType type, decimal price,
            int? quantity, OrderType orderType, HitBtcTimeInForce? timeInForce, CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();

            var parameters = new Dictionary<string, string>
            {
                {"nonce", Convert.ToString(GetNonce(), culture)},
                {"apikey", _apiKey},
                {"clientOrderId", clientOrderId},
                {"symbol", pair.ToString().Replace("/", string.Empty)},
                {"side", type.ToString()},
                {"price", Convert.ToString(price, culture)}
            };

            if (quantity.HasValue)
            {
                parameters.Add("quantity", quantity.Value.ToString());
            }

            parameters.Add("type", orderType.ToString());

            if (timeInForce.HasValue)
            {
                parameters.Add("timeInForce", timeInForce.Value.ToString());
            }

            return await RetryHelper.DoAsync(async () => await PrivateQuery<string>(
                $"api/{Version}/trading/cancel_order", parameters, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await GetPairs(token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<Pair>> GetPairs(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query<SymbolsResponse>(_baseUri.AbsoluteUri + $"api/{Version}/public/symbols", token);
                return result.Symbols.Select(x => new Pair(x.Commodity, x.Currency)).Where(x=>x!=null).ToList();
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<HitBtcTicker>> GetTickers(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject jobject = await JObjectQuery(_baseUri.AbsoluteUri + $"api/{Version}/public/ticker", token);
                return HitBtcTicker.GetFromJObject(jobject);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<HitBtcTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await Query<HitBtcTicker>(_baseUri.AbsoluteUri +
                                                     $"api/{Version}/public/{pair.ToString().Replace("/", string.Empty)}/ticker", token);
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<HitBtcOrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await Query<HitBtcOrderBook>(_baseUri.AbsoluteUri +
                                                        $"/api/{Version}/public/{pair.ToString().Replace("/", string.Empty)}/orderbook", token);
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
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
            catch (Exception ex)
            {
                //Log.Error("BitFinex: Can't parse json {0} to BitFinex<{1}>", resultData, typeof(T));
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

        #region Private Query
        private async Task<string> PrivateQuery(string url, Dictionary<string, string> parameters, CancellationToken token = default(CancellationToken))
        {
            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);
                var query = ToAmPersandEncoding(parameters);
                var fullUrl = _baseUri.AbsoluteUri + url + "?" + query;
                var signatureQuery = "/" + url + "?" + query;
                client.DefaultRequestHeaders.Add("X-Signature", CalculateSignature(signatureQuery, _secretKey));

                using (HttpResponseMessage response = await client.GetAsync(fullUrl, token))
                {
                    // Read response asynchronously 
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

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
                        //Log.Error("HitBtc: Can't parse json {0} to BitFinex<{1}>, URL - {2}, Exception Message - {3}", resultData, typeof(T), url, ex.Message);
                        throw;
                    }
                }, TimeSpan.FromMilliseconds(0), 10);
            }, token);
        }

        public static string CalculateSignature(string text, string secretKey)
        {
            using (var hmacsha512 = new HMACSHA512(Encoding.UTF8.GetBytes(secretKey)))
            {
                hmacsha512.ComputeHash(Encoding.UTF8.GetBytes(text));
                return string.Concat(hmacsha512.Hash.Select(b => b.ToString("x2")).ToArray()); // minimalistic hex-encoding and lower case
            }
        }
        #endregion

        public Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = new CancellationToken())
        {
            throw new System.NotImplementedException();
        }

        #region Utils
        private static long GetNonce()
        {
            return DateTime.UtcNow.Ticks * 10 / TimeSpan.TicksPerMillisecond; // use millisecond timestamp or whatever you want
        }

        private static string ToAmPersandEncoding(Dictionary<string, string> pairs)
        {
            IEnumerable<string> joinedPairs = pairs.Select(pair => WebUtility.UrlEncode(pair.Key) + "=" + WebUtility.UrlEncode(pair.Value)).ToList();
            return String.Join("&", joinedPairs);
        }
        #endregion

    }
}
