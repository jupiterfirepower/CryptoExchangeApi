using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Exchange.Cex.Responses;
using Exchange.Cex.Exceptions;
using Exchange.Cex.Model;

namespace Exchange.Cex
{
    public class CexApi : ApiBase, ICexApi
    {
        private readonly string _key;
        private readonly string _secret;

        protected string ApiUrl = "https://cex.io/api/";

        private readonly string _userName;

        #region Logger
        //protected readonly ILogger Log;
        #endregion

        public CexApi(string key, string secret, string userName)
        {
            _key = key;
            _secret = secret;
            _userName = userName;
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
                //Log.Error("Cex: Can't parse json {0} to Cex<{1}>", resultData, typeof(T));
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

        private const string NonceError = "{\"error\":\"Nonce must be incremented\"}";

        public async Task<T> PrivateQuery<T>(string url, Dictionary<string, string> parameters,
            CancellationToken token = default(CancellationToken),
            RequestCategory requestCategory = RequestCategory.Ignore)
        {
            string resultData = string.Empty;

            return await RetryHelper.DoAsync(async () =>
            {
                try
                {
                    resultData = await PrivateQuery(url, parameters, token, requestCategory);

                    return JsonConvert.DeserializeObject<T>(resultData);
                }
                catch (Exception ex)
                {
                    //Log.Warn("Cex: Can't parse json {0} to Cex<{1}>, URL - {2}, Exception Message - {3}, Nonce - {4}", resultData, typeof(T), url, ex.Message, parameters["nonce"]);
                    throw;
                }
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        private async Task<string> PrivateQuery(string url, Dictionary<string, string> parameters,
            CancellationToken token = default(CancellationToken),
            RequestCategory requestCategory = RequestCategory.Ignore)
        {
            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                using (HttpResponseMessage response = await client.PostAsync(new Uri(url), new FormUrlEncodedContent(parameters), token))
                {
                    // Read response asynchronously 
                    var data = await response.Content.ReadAsStringAsync();
                    CorrectNonce(data, parameters);
                   // FileStorageHelper.StoreFile(ExchangeName.Cex, url, ConvertorHelper.DictionaryToJson(parameters), data, requestCategory, Log);
                    return data;
                }
            }
        }

        private void CorrectNonce(string data, Dictionary<string, string> parameters)
        {
            if (data.Contains(NonceError))
            {
                var nonce = GetNonce();
                var signature = GetSignature(nonce, _userName, _key, _secret);

                parameters["nonce"] = Convert.ToString(nonce);
                parameters["signature"] = signature;
            }
        }

        #endregion

        #region Public Methods
        public async Task<CexTicker> GetTickers(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<CexTicker>(ApiUrl + $"ticker/{pair.BaseCurrency}/{pair.CounterCurrency}", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<CexLastPrice> GetLastPrice(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<CexLastPrice>(ApiUrl + $"last_price/{pair.BaseCurrency}/{pair.CounterCurrency}", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<CexTradeHistory>> GetTradeHistory(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<List<CexTradeHistory>>(ApiUrl + $"trade_history/{pair.BaseCurrency}/{pair.CounterCurrency}", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await Query<CexOrderBook>(ApiUrl + $"order_book/{pair.BaseCurrency}/{pair.CounterCurrency}", token);
                return FromCexOrderBook(data, pair);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        private OrderBook FromCexOrderBook(CexOrderBook orderBook, Pair pair)
        {
            const string exchangeName = ExchangeName.Cex;
            
            var ob = orderBook.Bids != null ? new OrderBook(
                orderBook.Bids.Where(x => x[0] > 0 && x[1] > 0).Select(b => FromCexOrder(exchangeName, pair, MarketSide.Bid, b[0], b[1])),
                orderBook.Asks.Where(x => x[0] > 0 && x[1] > 0).Select(b => FromCexOrder(exchangeName, pair, MarketSide.Ask, b[0], b[1])),
                exchangeName,
                pair,
                DateTime.UtcNow) : null;
            return ob;
        }

        public Order FromCexOrder(string exchangeName, Pair pair, MarketSide marketSide, decimal price, decimal amount)
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

        public async Task<CexBalance> GetBalances(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var data = await RetryHelper.DoAsync(async () =>
                {
                    var nonce = GetNonce();
                    var signature = GetSignature(nonce, _userName, _key, _secret);

                    return await PrivateQuery(ApiUrl + "balance/", new Dictionary<string, string>
                    {
                        { "key", _key },
                        { "signature", signature.ToUpper() },
                        { "nonce", Convert.ToString(nonce) }
                    }, token, RequestCategory.AccountHoldings);

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));

                var jobj = JObject.Parse(data);
                var result = jobj.ToObject<CexBalance>();
                var balances = CexAccountBalance.GetFromJObject(jobj);
                result.AccountBalances = balances;

                return result;
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var accountHoldings = await GetBalances(token);

                return accountHoldings.AccountBalances.Select(ah => new AccountChange(ExchangeName.Cex,
                                ah.Currency,
                                ah.AvailableAmount)); // there is also 'balance' available

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    if (order == null)
                        throw new ArgumentNullException(nameof(order), "Cex SellLimit method.");

                    if (order.MarketSide != MarketSide.Ask)
                        throw new CexException("Cex API, Incorrect market side: {0}".FormatAs(order));

                    var neworder = await PlaceOrder(PairConvertorHelper.DashToDrkPair(order.Pair), OrderSide.Sell, order.Amount, decimal.Round(order.Price, 4), token);

                    if (string.IsNullOrEmpty(neworder.Id) || !string.IsNullOrEmpty(neworder.Error))
                        throw new CexException("Cex API, Order rejected: {0}".FormatAs(order));

                    return Convert.ToString(neworder.Id);

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    if (order == null)
                        throw new ArgumentNullException(nameof(order), "Cex BuyLimit method.");

                    if (order.MarketSide != MarketSide.Bid)
                        throw new CexException("Cex API, Incorrect market side: {0}".FormatAs(order));

                    var neworder = await PlaceOrder(PairConvertorHelper.DashToDrkPair(order.Pair), OrderSide.Buy, order.Amount, decimal.Round(order.Price, 4), token);

                    if (string.IsNullOrEmpty(neworder.Id) || !string.IsNullOrEmpty(neworder.Error))
                        throw new CexException("Cex API, Order rejected: {0}".FormatAs(order));

                    return Convert.ToString(neworder.Id);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await SellLimit(order, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await BuyLimit(order, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<IEnumerable<CexOpenOrder>> GetOpenOrders(Pair pair, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var data = await RetryHelper.DoAsync(async () =>
                {
                    var nonce = GetNonce();
                    var signature = GetSignature(nonce, _userName, _key, _secret);

                    return await PrivateQuery<List<CexOpenOrder>>(ApiUrl + "open_orders/" + PairConvertorHelper.DashToDrkPair(pair), new Dictionary<string, string>
                    {
                        { "key", _key },
                        { "signature", signature.ToUpper() },
                        { "nonce", Convert.ToString(nonce) }
                    }, token, RequestCategory.OpenOrders);

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);

                return data;
            }
            finally
            {
                AutoResetEventSet();
            }
            
        }

        private const int LimitMax = 100;

        public async Task<IEnumerable<CexOrderArchive>> GetArchivedOrders(Pair pair, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var data = await RetryHelper.DoAsync(async () =>
                {
                    var nonce = GetNonce();
                    var signature = GetSignature(nonce, _userName, _key, _secret);

                    return await PrivateQuery<List<CexOrderArchive>>(
                                ApiUrl + "archived_orders/" + PairConvertorHelper.DashToDrkPair(pair),
                                new Dictionary<string, string>
                                {
                                    { "key", _key },
                                    { "signature", signature.ToUpper() },
                                    { "nonce", Convert.ToString(nonce) },
                                    { "limit", Convert.ToString(LimitMax) },
                                    { "dateFrom", Convert.ToString(UnixTime.GetNowAddDays(-3)) },
                                    { "dateTo", Convert.ToString(UnixTime.GetNowAddDays(1)) }
                                }, token, RequestCategory.OrderHistory);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);

                return data;
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<bool> CancelOrder(string orderId, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var data = await RetryHelper.DoAsync(async () =>
                {
                    var nonce = GetNonce();
                    var signature = GetSignature(nonce, _userName, _key, _secret);

                    return await PrivateQuery(ApiUrl + "cancel_order/", new Dictionary<string, string>
                    {
                        { "key", _key },
                        { "signature", signature.ToUpper() },
                        { "nonce", Convert.ToString(nonce) },
                        { "id", orderId }
                    }, token, RequestCategory.CancelOrder);
                } , TimeSpan.FromMilliseconds(Constant.CexRetryInterval));

                return bool.Parse(data);
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<CexPlaceOrderResponse> PlaceOrder(Pair pair, OrderSide type, decimal amount, decimal price, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var culture = CultureHelper.GetEnglishCulture();

                var data = await RetryHelper.DoAsync(async () =>
                {
                    var nonce = GetNonce();
                    var signature = GetSignature(nonce, _userName, _key, _secret);

                    return await PrivateQuery<CexPlaceOrderResponse>(ApiUrl + "place_order/" + pair, new Dictionary<string, string>
                    {
                        { "key", _key },
                        { "signature", signature.ToUpper() },
                        { "nonce", Convert.ToString(nonce) },
                        { "type", type.ToString().ToLower() },
                        { "amount", Convert.ToString(amount, culture) },
                        { "price", Convert.ToString(price, culture) }
                    }, token, RequestCategory.SubmitOrder);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);

                return data;
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<Pair>> GetSupportedPairs()
        {
            return await Task<IEnumerable<Pair>>.Factory.StartNew(() =>
            {
                var list = new List<Pair>
                {
                    new Pair(SupportedCurrency.BTC, SupportedCurrency.USD),
                    new Pair(SupportedCurrency.BTC, SupportedCurrency.EUR),
                    new Pair(SupportedCurrency.BTC, SupportedCurrency.GBP),
                    new Pair(SupportedCurrency.BTC, SupportedCurrency.RUB),

                    new Pair(SupportedCurrency.ETH, SupportedCurrency.BTC),
                    new Pair(SupportedCurrency.ETH, SupportedCurrency.USD),
                    new Pair(SupportedCurrency.ETH, SupportedCurrency.EUR),

                    new Pair(SupportedCurrency.LTC, SupportedCurrency.USD),
                    new Pair(SupportedCurrency.LTC, SupportedCurrency.BTC),
                    new Pair(SupportedCurrency.GHS, SupportedCurrency.BTC)
                };

                return list.AsEnumerable();
            });
        }

        #region Utils
        private static long _lastNonce;

        private static long GetNonce()
        {
            var currentNonce = DateTime.UtcNow.Ticks;
            _lastNonce = currentNonce = currentNonce > _lastNonce ? currentNonce : (currentNonce + Math.Abs(_lastNonce - currentNonce) + 1);
            return currentNonce;
        }

        private static string GetSignature(long nonce, string userName, string apiKey, string secret)
        {
            var message = Convert.ToString(nonce) + userName + apiKey;
            var signature = Sign(message, secret);
            return signature;
        }

        private static string Sign(string postData, string secretKey)
        {
            return ByteArrayToString(SignHMacSha256(secretKey, Encoding.UTF8.GetBytes(postData)));
        }

        private static byte[] SignHMacSha256(string key, byte[] data)
        {
            var hashMaker = new HMACSHA256(Encoding.ASCII.GetBytes(key));
            return hashMaker.ComputeHash(data);
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        #endregion

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var openOrders = await GetOpenOrders(order.Pair, token);
                var openOrder = openOrders.SingleOrDefault(x => x.Id == order.Id && x.PendingAmount > 0);

                if (openOrder != null)
                {
                    return Tuple.Create(openOrder.Price, openOrder.PendingAmount);
                }

                var archiveOrder = (await GetArchivedOrders(order.Pair, token)).SingleOrDefault(x => x.OrderId == order.Id);

                if (archiveOrder != null && archiveOrder.OrderStatus == OrderStatus.Filled)
                {
                    return Tuple.Create(archiveOrder.Price, archiveOrder.OrderStatus == OrderStatus.Filled ? order.Amount : archiveOrder.Amount);
                }

                return Tuple.Create(-1m, -1m);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "Cex GetOrderStatus method.");

                if (!string.IsNullOrEmpty(order.Id))
                {
                    var openOrders = await GetOpenOrders(order.Pair, token);
                    var openOrder = openOrders.SingleOrDefault(x => x.Id == order.Id && x.PendingAmount > 0);

                    if (openOrder != null)
                    {
                        return new OrderChange(order, 0, openOrder.Price, OrderStatus.PartiallyFilled, DateTime.UtcNow, openOrder.PendingAmount);
                    }

                    var archiveOrders = (await GetArchivedOrders(order.Pair, token)).ToList();
                    var archiveOrder = archiveOrders.SingleOrDefault(x => x.OrderId == order.Id);

                    if (archiveOrder != null && archiveOrder.OrderStatus == OrderStatus.Filled)
                    {
                        return new OrderChange(order, order.Amount, archiveOrder.Price, OrderStatus.Filled, DateTime.UtcNow, order.Amount);
                    }
                }

                return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }
    }
}
