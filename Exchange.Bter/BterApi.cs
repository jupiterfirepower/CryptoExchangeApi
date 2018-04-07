using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Exchange.Bter.Enums;
using Exchange.Bter.Exceptions;
using Exchange.Bter.Model;
using Exchange.Bter.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Common.Contracts;
using System.Numerics;
using System.Configuration;

namespace Exchange.Bter
{
    public class BterApi : ApiBase, IBterApi
    {
        private readonly string _key;
        private readonly string _secret;

        #region Private Constants
        private readonly Uri _baseUri = new Uri(@"https://data.bter.com/");
        private readonly Uri _basePrivateUri = new Uri(@"https://bter.com/");
        private const string  Version = "1";
        private readonly string _apiVersion = $"api/{Version}/";
        #endregion

        #region Constructor
        public BterApi(string apiKey, string apiSecret)
        {
            _key = apiKey;
            _secret = apiSecret;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["BterMultiplier"]))
            {
                BigInteger.TryParse(ConfigurationManager.AppSettings["BterMultiplier"], out _multiplier);
            }
            _multiplier = _multiplier <= 0 ? 1 : _multiplier;
        }

        #endregion

        #region Private Api

        public async Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var accountHoldings = BterAccountBalance.GetFromJObject(await GetBalance(token));
                return accountHoldings.Select(ah => new AccountChange(ExchangeName.Bter, ah.Currency,
                                ah.AvailableAmount)); // there is also 'balance' available*/
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<BterResponse> CancelOrder(string orderId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (string.IsNullOrEmpty(orderId))
                    throw new ArgumentNullException(nameof(orderId), "BterApi CancelOrder method.");

                var result = await PrivateQuery("cancelorder", new NameValueCollection
                {
                    { "nonce", GetNonce().ToString("D") },
                    { "order_id", orderId },
                }, _key, _secret, token, RequestCategory.CancelOrder);

                return result.JObject.ToObject<BterResponse>();
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BterApi BuyLimit method.");

                if (order.MarketSide != MarketSide.Bid)
                    throw new BterException("Bter API, Incorrect market side: {0}".FormatAs(order));

                return await PlaceOrder(order, BterOrderType.Buy, token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BterApi SellLimit method.");

                if (order.MarketSide != MarketSide.Ask)
                    throw new BterException("Bter API, Incorrect market side: {0}".FormatAs(order));

                return await PlaceOrder(order, BterOrderType.Sell, token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> PlaceOrder(Order order, BterOrderType type, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BterApi PlaceOrder method.");

                var result = await PrivateQuery("placeorder", new NameValueCollection
                {
                    { "nonce", GetNonce().ToString("D") },
                    { "pair", ToBterPair(order.Pair) },
                    { "type", type.ToString().ToLower() }, //sell, buy
                    { "rate", Convert.ToString(order.Price, CultureHelper.GetEnglishCulture().NumberFormat) },
                    { "amount", Convert.ToString(order.Amount, CultureHelper.GetEnglishCulture().NumberFormat) },
                }, _key, _secret, token, RequestCategory.SubmitOrder);

                if (!result.BterResponse.Result || !result.BterResponse.IsSuccess)
                {
                    throw new BterException("Bter API, PlaceOrder rejected: {0}, Message - {1}".FormatAs(order, result.BterResponse.Message ?? result.BterResponse.Msg));
                }

                return result.BterResponse.OrderId;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<BterOrderStatus> GetOrder(string orderId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                BterOrderStatus orderStatus = null;

                if (string.IsNullOrEmpty(orderId))
                {
                    throw new ArgumentNullException(nameof(orderId), "BterApi GetOrder method.");
                }

                var result = await PrivateQuery("getorder", new NameValueCollection
                {
                    { "nonce", GetNonce().ToString("D") },
                    { "order_id", orderId }
                }, _key, _secret, token, RequestCategory.OrderStatus);

                if (!result.BterResponse.Result || !result.BterResponse.IsSuccess)
                {
                    throw new BterException("Bter API, GetOrder: orderId - {0}, Message -{1}".FormatAs(orderId, result.BterResponse.Message ?? result.BterResponse.Msg));
                }

                var jobject = result.JObject;

                if (jobject["order"] != null)
                {
                    orderStatus = jobject["order"].ToObject<BterOrderStatus>();
                }

                return orderStatus;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var orderState = await GetOrder(order.Id, token);
                var totalFillAmount = orderState.Amount;
                var avgPrice = orderState.Rate;
                return Tuple.Create(avgPrice, totalFillAmount);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BterApi GetOrderStatus method.");

                if (!string.IsNullOrEmpty(order.Id))
                {
                    var orderStatus = await GetOrder(order.Id, token);

                    BterStatusOrder ostatus;
                    Enum.TryParse(orderStatus.Status, true, out ostatus);

                    var status = RemapStatus(ostatus);

                    switch (status)
                    {
                        case OrderStatus.Filled:
                            return new OrderChange(order, orderStatus.InitialAmount, orderStatus.Rate, OrderStatus.Filled, DateTime.UtcNow, orderStatus.InitialAmount);

                        case OrderStatus.PartiallyFilled:
                            return new OrderChange(order, 0, orderStatus.Rate, OrderStatus.PartiallyFilled, DateTime.UtcNow, orderStatus.Amount);

                        //case OrderStatus.Canceled:
                        //    return new OrderChange(order, 0, orderStatus.Rate, OrderStatus.Canceled, DateTime.UtcNow, orderStatus.Amount);

                        case OrderStatus.Rejected:
                            return new OrderChange(order, 0, orderStatus.Rate, OrderStatus.Rejected, DateTime.UtcNow, orderStatus.Amount);
                    }
                }

                return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<JObject> GetBalance(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await PrivateQuery("getfunds", new NameValueCollection
                {
                    { "nonce", GetNonce().ToString("D") }
                }, _key, _secret, token, RequestCategory.AccountHoldings);

                return result.JObject;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<ConcurrentBag<BterOrder>> GetOrderList(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await PrivateQuery("orderlist", new NameValueCollection
                {
                    { "nonce", GetNonce().ToString("D") }
                }, _key, _secret, token, RequestCategory.OpenOrders);

                return BterOrder.GetFromJObject(result.JObject);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #endregion

        #region Private Special
        private OrderStatus RemapStatus(BterStatusOrder status)
        {
            switch (status)
            {
                case BterStatusOrder.Open:
                    return OrderStatus.New;
                case BterStatusOrder.Cancelled:
                    return OrderStatus.Canceled;
                case BterStatusOrder.Rejected:
                    return OrderStatus.Rejected;
                case BterStatusOrder.Filled:
                    return OrderStatus.Filled;
                case BterStatusOrder.PartFilled:
                    return OrderStatus.Filled;
                case BterStatusOrder.Complete:
                    return OrderStatus.Filled;
                case BterStatusOrder.Completed:
                    return OrderStatus.Filled;
                case BterStatusOrder.FullFill:
                    return OrderStatus.Filled;
                case BterStatusOrder.Full_Fill:
                    return OrderStatus.Filled;
                case BterStatusOrder.PartialFill:
                    return OrderStatus.PartiallyFilled;
                case BterStatusOrder.Partial_Fill:
                    return OrderStatus.PartiallyFilled;
                default:
                    return OrderStatus.Unknown;
            }
        }

        private OrderBook FromBterOrderBook(BterOrderBook bterOrderBook, Pair pair)
        {
            var culture = CultureHelper.GetEnglishCulture();
            if (bterOrderBook.Bids != null && bterOrderBook.Asks != null)
            {
                return new OrderBook(
                    bterOrderBook.Bids.Select(x => FromBterOrder(pair, MarketSide.Bid, Convert.ToDecimal(x[0], culture), Convert.ToDecimal(x[1], culture))),
                    bterOrderBook.Asks.Select(x => FromBterOrder(pair, MarketSide.Ask, Convert.ToDecimal(x[0], culture), Convert.ToDecimal(x[1], culture))),
                    ExchangeName.Bter,
                    pair,
                    DateTime.UtcNow);
            }

            return null;
        }

        private Order FromBterOrder(Pair pair, MarketSide marketSide, decimal price, decimal amount)
        {
            return new Order(
                pair,
                price,
                amount,
                ExchangeName.Bter,
                marketSide,
                DateTime.UtcNow,
                OrderType.Limit,
                SourceSystemCode.ExternalExchange);
        }
        #endregion

        #region Public Api
        public async Task<ConcurrentBag<BterPairInfo>> GetPairs(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject jobject = await Query(_apiVersion + "marketinfo", null, token);
                return BterPairInfo.GetFromJObject(jobject);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<ConcurrentBag<BterMarketInfo>> GetMarketInfo(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject jobject = await Query(_apiVersion + "marketlist", null, token);
                return BterMarketInfo.GetFromJObject(jobject);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<ConcurrentBag<BterTicker>> GetTickers(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject jobject = await Query(_apiVersion + "tickers", null, token);
                return BterTicker.GetFromJObject(jobject);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<BterTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (pair == null)
                    throw new ArgumentNullException(nameof(pair), "BterApi GetTicker method.");

                string bterPair = ToBterPair(pair);
                JObject jobject = await Query(_apiVersion + "ticker/" + bterPair, null, token);
                var data = jobject.ToObject<BterTicker>();
                data.Pair = pair;
                data.PairStr = bterPair;
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<List<BterTradeHistory>> GetTradeHistory(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (pair == null)
                    throw new ArgumentNullException(nameof(pair), "BterApi GetTradeHistory method.");

                string bterPair = ToBterPair(pair);
                JObject jobject = await Query(_apiVersion + "trade/" + bterPair, null, token);
                var data = jobject["data"].ToObject<List<BterTradeHistory>>();
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (pair == null)
                    throw new ArgumentNullException(nameof(pair), "BterApi GetOrderBook method.");

                string bterPair = ToBterPair(pair);
                JObject jobject = await Query(_apiVersion + "depth/" + bterPair, null, token);
                var orderBook = jobject.ToObject<BterOrderBook>();
                return FromBterOrderBook(orderBook, pair);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #endregion

        #region Query

        private async Task<BterPrivateResult> PrivateQuery(string path, NameValueCollection req, string key, string secret, 
            CancellationToken token = default(CancellationToken), RequestCategory requestCategory = RequestCategory.Ignore)
        {
            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);
                string reqToString = EncodeParameters(req);

                client.DefaultRequestHeaders.Add("KEY", key);
                client.DefaultRequestHeaders.Add("SIGN", Sign(reqToString, secret));

                var content = new FormUrlEncodedContent(req.AsKeyValuePair());
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using (var response = await client.PostAsync(_basePrivateUri.AbsoluteUri + _apiVersion + "private/" + path, content, token))
                {
                    var data = await response.Content.ReadAsStringAsync();
                    try
                    {
                       // FileStorageHelper.StoreFile(ExchangeName.Bter, path, ConvertorHelper.DictionaryToJson(req), data, requestCategory, Log);

                        var bterResponse = JsonConvert.DeserializeObject<BterResponse>(data);

                        return new BterPrivateResult
                        {
                            BterResponse = bterResponse,
                            JObject = JObject.Parse(data)
                        };
                    }
                    catch (WebException wex)
                    {
                        //Log.Error(wex);

                        return new BterPrivateResult
                        {
                            BterResponse = new BterResponse(),
                            JObject = null
                        };
                    }
                }
            }
        }

        public async Task<JObject> Query(string uri, Dictionary<string, string> args = null, CancellationToken token = default(CancellationToken))
        {
            var responseData = await PublicQuery(uri, args, token);
            try
            {
                JObject jobject = JObject.Parse(responseData);

                var responseObject = JsonConvert.DeserializeObject<BterResponse>(responseData);

                if (responseObject == null)
                {
                    throw new BterException("Bter API Exception: Json Can't parse result string.");
                }

                return jobject;
            }
            catch (Exception)
            {
                //Log.Warn("Bter: Can't parse json {0} to BterResponse<{1}>", responseData, typeof(BterResponse));
                throw;
            }
        }

        public async Task<string> PublicQuery(string uri, Dictionary<string, string> args = null, CancellationToken token = default(CancellationToken))
        {
            string queryStr = string.Empty;

            queryStr += args == null ? string.Empty : "&" + EncodeParameters(args);
            uri = uri + "?" + queryStr;

            var fullUri = new Uri(_baseUri, new Uri(uri, UriKind.Relative));

            return await QueryHelper.Query(fullUri.AbsoluteUri, token);
        }

        #endregion

        #region Util methods
        private static BigInteger _multiplier = 1;
        private static BigInteger _lastNonce = 0;

        private BigInteger GetNonce()
        {
            var currentNonce = new BigInteger(DateTime.UtcNow.Ticks) * _multiplier;
            if (currentNonce > _lastNonce)
            {
                _lastNonce = currentNonce;
            }
            else
            {
                var diff = _lastNonce - currentNonce;
                currentNonce += diff + 1;
                _lastNonce = currentNonce;
            }
            return currentNonce;
        }

        private string Sign(string reqToString, string secret)
        {
            var sBuilder = new StringBuilder();
            var keyByte = Encoding.UTF8.GetBytes(secret);

            using (var hmac = new HMACSHA512(keyByte))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(reqToString));
                foreach (byte t in hash)
                {
                    sBuilder.Append(t.ToString("x2"));
                }
            }
            return sBuilder.ToString();
        }

        private static string EncodeParameters(NameValueCollection req)
        {
            // Convert from NameValueCollection to string ready for post.
            var parameters = new StringBuilder();

            foreach (string reqKey in req.Keys)
            {
                parameters.AppendFormat("{0}={1}&", WebUtility.UrlEncode(reqKey), WebUtility.UrlEncode(req[reqKey]));
            }

            if (parameters.Length > 0)
            {
                parameters.Length -= 1;
            }

            return parameters.ToString();
        }

        private static string EncodeParameters(Dictionary<string, string> parameters)
        {
            return string.Join("&", parameters.Select(EncodeParameter).ToArray());
        }
        private static string EncodeParameter(KeyValuePair<string, string> parameter)
        {
            if (parameter.Value == null)
            {
                return string.Concat(WebUtility.UrlEncode(parameter.Key), "=");
            }
            return string.Concat(WebUtility.UrlEncode(parameter.Key), "=", WebUtility.UrlEncode(parameter.Value));
        }

        private string ToBterPair(Pair pair)
        {
            // Attention: Bittrex pair is reversed!!!
            return "{0}_{1}".FormatAs(pair.BaseCurrency.ToLower(), pair.CounterCurrency.ToLower());
        }
        #endregion Util methods

    }
}
