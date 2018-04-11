using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Poloniex.Model;
using Exchange.Poloniex.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.Poloniex.Exceptions;


namespace Exchange.Poloniex
{
    public class PoloniexApi : ApiBase, IPoloniexApi
    {
        private readonly string _key;
        private readonly string _secret;
        
        protected string TradingUrl = "https://poloniex.com/tradingApi";
        protected string PublicUrl  = "https://poloniex.com/public";

        #region Logger
        //protected readonly ILogger Log;
        #endregion

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
                //Log.Error("Poloniex: Can't parse json {0} to Poloniex<{1}>", resultData, typeof(T));
                throw;
            }
        }

        private async Task<JArray> JArrayQuery(string url, CancellationToken token = default(CancellationToken))
        {
            var data = await Query(url, token);
            return JArray.Parse(data);
        }

        private async Task<JObject> JObjectQuery(string uri, CancellationToken token = default(CancellationToken))
        {
            var data = await Query(uri, token);
            return JObject.Parse(data);
        }
        #endregion

        public PoloniexApi(string key, string secret)
        {
            _key = key;
            _secret = secret;
        }

        #region Private Query Methods
        private async Task<T> PrivateQuery<T>(string url, Dictionary<string, string> parameters, 
            CancellationToken token = default(CancellationToken),
            RequestCategory requestCategory = RequestCategory.Ignore)
        {
            string resultData = String.Empty;

            return await RetryHelper.DoAsync(async () =>
            {
                try
                {
                    resultData = await PrivateQuery(url, parameters, token, requestCategory);

                    return JsonConvert.DeserializeObject<T>(resultData);
                }
                catch (Exception ex)
                {
                    //Log.Warn("Poloniex: Can't parse json {0} to Poloniex<{1}>, URL - {2}, Exception Message - {3}, Nonce - {4}", resultData, typeof(T), url, ex.Message, parameters["nonce"]);
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

                client.DefaultRequestHeaders.Add("Key", _key);
                client.DefaultRequestHeaders.Add("Sign", Sign(ToAmPersandEncoding(parameters), _secret));
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                using (HttpResponseMessage response = await client.PostAsync(new Uri(url), new FormUrlEncodedContent(parameters), token))
                {
                    var data = await response.Content.ReadAsStringAsync();
                    CorrectNonce(data, parameters);
                    //FileStorageHelper.StoreFile(ExchangeName.Poloniex, url, ConvertorHelper.DictionaryToJson(parameters), data, requestCategory, Log);
                    // Read response asynchronously 
                    return data;
                }
            }
        }

        private void CorrectNonce(string data, Dictionary<string, string> parameters)
        {
            if (data.StartsWith(NonceError))
            {
                parameters["nonce"] = Convert.ToString(GetNonce());
            }
        }

        private const string NonceError = "{\"error\":\"Nonce must be greater than";
        #endregion

        #region Private Methods

        public async Task<IEnumerable<PoloniexBalance>> GetBalances(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var data = await RetryHelper.DoAsync(async () => await PrivateQuery(TradingUrl, new Dictionary<string, string>
                {
                    { "command", "returnBalances" },
                    { "nonce", Convert.ToString(GetNonce()) }
                }, token, RequestCategory.AccountHoldings), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));

                return PoloniexBalance.GetFromJObject(JObject.Parse(data));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<PoloniexCompleteBalance>> GetCompleteBalances(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var data = await RetryHelper.DoAsync(async () => await PrivateQuery(TradingUrl, new Dictionary<string, string>
                {
                    { "command", "returnCompleteBalances" },
                    { "nonce", Convert.ToString(GetNonce()) }
                }, token, RequestCategory.AccountHoldings), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));

                return PoloniexCompleteBalance.GetFromJObject(JObject.Parse(data));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<string> GetDepositAddresses(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () => await PrivateQuery(TradingUrl, new Dictionary<string, string>
                {
                    { "command", "returnDepositAddresses" },
                    { "nonce", Convert.ToString(GetNonce()) }
                }, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<string> GetGenerateNewAddress(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () => await PrivateQuery(TradingUrl, new Dictionary<string, string>
                {
                    { "command", "generateNewAddress" },
                    { "nonce", Convert.ToString(GetNonce()) }
                }, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<PoloniexOpenOrder>> GetOpenOrders(Pair pair, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () => await PrivateQuery<List<PoloniexOpenOrder>>(TradingUrl, new Dictionary<string, string>
                {
                    { "command", "returnOpenOrders" },
                    { "nonce", Convert.ToString(GetNonce()) },
                    { "currencyPair", PairToString(pair) },
                    { "trades", "true" }
                }, token, RequestCategory.OpenOrders), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<PoloniexOrderHistory>> GetTradeHistory(Pair pair, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () => await PrivateQuery<List<PoloniexOrderHistory>>(TradingUrl, new Dictionary<string, string>
                {
                    { "command", "returnTradeHistory" },
                    { "nonce", Convert.ToString(GetNonce()) },
                    { "currencyPair", PairToString(pair)  }
                }, token, RequestCategory.OrderHistory), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<CancelOrderResponse> CancelOrder(string orderNumber, Pair pair, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () => await PrivateQuery<CancelOrderResponse>(TradingUrl, new Dictionary<string, string>
                {
                    { "command", "cancelOrder" },
                    { "nonce", Convert.ToString(GetNonce()) },
                    { "currencyPair", PairToString(pair) },
                    { "orderNumber", orderNumber }
                }, token, RequestCategory.CancelOrder), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        private async Task<CreateOrderResponse> CreateOrder(string command, Pair pair, decimal rate, decimal amount, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                var culture = CultureHelper.GetEnglishCulture();

                return await RetryHelper.DoAsync(async () => await PrivateQuery<CreateOrderResponse>(TradingUrl, new Dictionary<string, string>
                {
                    { "command", command },
                    { "nonce", Convert.ToString(GetNonce()) },
                    { "currencyPair", PairToString(pair) },
                    { "rate", Convert.ToString(rate, culture.NumberFormat) },
                    { "amount", Convert.ToString(amount, culture.NumberFormat) }
                }, token, RequestCategory.SubmitOrder), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<CreateOrderResponse> NewOrder(Order order, OrderSide orderSide, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await CreateOrder(orderSide.ToString().ToLower(), order.Pair, order.Price, order.Amount, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "Poloniex SellLimit method.");

                if (order.MarketSide != MarketSide.Ask)
                    throw new PoloniexException("Poloniex API, Incorrect market side: {0}".FormatAs(order));

                var neworder = await NewOrder(order, OrderSide.Sell, token);

                if (neworder.OrderNumber <= 0)
                    throw new PoloniexException("Poloniex API, Order rejected: {0}".FormatAs(order));

                return Convert.ToString(neworder.OrderNumber);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "Poloniex BuyLimit method.");

                if (order.MarketSide != MarketSide.Bid)
                    throw new PoloniexException("Poloniex API, Incorrect market side: {0}".FormatAs(order));

                var neworder = await NewOrder(order, OrderSide.Buy, token);

                if (neworder.OrderNumber <= 0)
                    throw new PoloniexException("Poloniex API, Order rejected: {0}".FormatAs(order));

                return Convert.ToString(neworder.OrderNumber);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await SellLimit(order, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await BuyLimit(order, token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var accountHoldings = await GetCompleteBalances(token);

                return accountHoldings.Select(ah => new AccountChange(ExchangeName.Poloniex,
                                ah.Currency,
                                ah.Available)); // there is also 'balance' available
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        #endregion

        #region Public Methods
        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await Query<PoloniexOrderBook>(PublicUrl + "?command=returnOrderBook&currencyPair=" + pair.ToString().Replace("/", "_"), token);
                return FromPoloniexOrderBook(data, pair);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<PoloniexTicker>> GetTickers(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject jobject = await JObjectQuery(PublicUrl + "?command=returnTicker", token);
                return PoloniexTicker.GetFromJObject(jobject);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken)) 
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var data = await GetTickers(token);

                var results = data.ToList().Select(x =>
                {
                    var pairs = x.PairStr.Split('_');

                    var supported = SupportedCurrencyHelper.GetSupportedCurrencies();

                    var inCurrency = supported.FirstOrDefault(curency => curency == pairs[0].ToUpper());
                    var outCurrency = supported.FirstOrDefault(curency => curency == pairs[1].ToUpper());

                    if (inCurrency != null && outCurrency != null)
                    {
                        return new Pair(inCurrency, outCurrency);
                    }

                    return null;
                });

                return results.Where(n => n != null).Distinct().ToList();

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        private OrderBook FromPoloniexOrderBook(PoloniexOrderBook orderBook, Pair pair)
        {
            const string exchangeName = ExchangeName.Poloniex;
            var ob = new OrderBook(
                orderBook.Bids.Where(x => x[0] > 0 && x[1] > 0).Select(b => FromPoloniexOrder(exchangeName, pair, MarketSide.Bid, b[0], b[1])),
                orderBook.Asks.Where(x => x[0] > 0 && x[1] > 0).Select(b => FromPoloniexOrder(exchangeName, pair, MarketSide.Ask, b[0], b[1])),
                exchangeName,
                pair,
                DateTime.UtcNow);
            return ob;
        }

        public Order FromPoloniexOrder(string exchangeName, Pair pair, MarketSide marketSide, decimal price, decimal amount)
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

        public async Task<string> GetTradeHistoryData(Pair pair, CancellationToken token = default(CancellationToken)) 
        {
            return await RetryHelper.DoAsync(async () => await Query(PublicUrl + "?command=returnTradeHistory&currencyPair=" + pair.ToString().Replace("/", "_"), token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<PoloniexCurrency>> GetCurrencies(CancellationToken token = default(CancellationToken)) 
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject jobject = await JObjectQuery(PublicUrl + "?command=returnCurrencies", token);
                return PoloniexCurrency.GetFromJObject(jobject);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<GetVolumeResponse> GetVolume(Pair pair, CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();
           
            var data = await Query(PublicUrl + "?command=return24hVolume", token);
            var result = JsonConvert.DeserializeObject<GetVolumeResponse>(data);

            var jobj = JObject.Parse(data);
            var resultList = new List<PoloniexVolume>();
            jobj.OfType<JProperty>().Select(x =>
            {
                try
                {
                    return new PoloniexVolume
                    {
                        PairStr = x.Name,
                        VolumeA = Convert.ToDecimal(x.Value.First.Last.ToString(), culture),
                        VolumeB = Convert.ToDecimal(x.Value.Last.Last.ToString(), culture)
                    };
                }
                catch
                {
                    // ignored
                }
                return null;
            }).Where(x => x != null).ToList().ForEach(resultList.Add);
            
            result.Volumes = resultList;
            return result;
        }
        #endregion

        #region Utils
        private static long _lastNonce;

        private static long GetNonce()
        {
            var currentNonce = DateTime.UtcNow.Ticks;
            _lastNonce = currentNonce = currentNonce > _lastNonce ? currentNonce : (currentNonce + Math.Abs(_lastNonce - currentNonce) + 1);
            return currentNonce;
        }

        private string Sign(string postData, string secretKey)
        {
            return ByteArrayToString(SignHMacSha512(secretKey, Encoding.UTF8.GetBytes(postData)));
        }

        private static byte[] SignHMacSha512(string key, byte[] data)
        {
            var hashMaker = new HMACSHA512(Encoding.ASCII.GetBytes(key));
            return hashMaker.ComputeHash(data);
        }

        private static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static string ToAmPersandEncoding(Dictionary<string, string> pairs)
        {
            IEnumerable<string> joinedPairs = pairs.Select(pair => WebUtility.UrlEncode(pair.Key) + "=" + WebUtility.UrlEncode(pair.Value)).ToList();
            return String.Join("&", joinedPairs);
        }

        private static string PairToString(Pair pair)
        {
            return String.Format("{0}_{1}", pair.BaseCurrency, pair.CounterCurrency).ToUpper();
        }
        #endregion

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                decimal totalFilledAmount = -1;
                decimal avgPrice = -1;
                var trades = (await GetTradeHistory(order.Pair, token)).ToList();

                if (trades.Any(x => x.OrderNumber == order.Id))
                {
                    totalFilledAmount = trades.Sum(x => x.Amount);
                    avgPrice = trades.Average(x => x.Rate);
                }

                return Tuple.Create(avgPrice, totalFilledAmount);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "Poloniex GetOrderStatus method.");

                if (!string.IsNullOrEmpty(order.Id))
                {
                    var trades = (await GetTradeHistory(order.Pair, token)).ToList();

                    if (trades.Any(x => x.OrderNumber == order.Id))
                    {
                        var amount = trades.Sum(x => x.Amount);
                        var avgPrice = trades.Average(x => x.Rate);

                        if (amount == order.Amount)
                            return new OrderChange(order, amount, avgPrice, OrderStatus.Filled, DateTime.UtcNow, amount);

                        if (amount < order.Amount)
                            return new OrderChange(order, 0, avgPrice, OrderStatus.PartiallyFilled, DateTime.UtcNow, amount);
                    }
                }

                return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }
    }
}
