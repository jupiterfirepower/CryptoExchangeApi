using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using BitFinex.Responses;
using BitFinex.Model;
using BitFinex.Enums;
using BitFinex.Exceptions;
using Common.Contracts;

namespace BitFinex
{
    public class BitFinexApi : ApiBase, IBitFinexApi
    {
        private readonly string _key;
        private readonly string _secret;

        #region Private
        private readonly Uri _baseUri = new Uri("https://api.bitfinex.com/");
        private const string Version = "1";
        #endregion

        public BitFinexApi(string apiKey, string apiSecret)
        {
            _key = apiKey;
            _secret = apiSecret;
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
                if (!string.IsNullOrEmpty(resultData))
                    ;
                    //Log.Warn("BitFinex: Can't parse json {0} to BitFinex<{1}>", resultData, typeof(T));
                throw;
            }
        }

        private async Task<JArray> JObjectQuery(string url, CancellationToken token = default(CancellationToken))
        {
            var data = await Query(url, token);
            return JArray.Parse(data);
        }
        #endregion

        #region Private Query Methods

        private const string NonceError = "Nonce is too small.";

        public async Task<T> PrivateQuery<T>(string url, Dictionary<string, object> parameters,
            CancellationToken token = default(CancellationToken),
            RequestCategory requestCategory = RequestCategory.Ignore)
        {
            string resultData = string.Empty;

            try
            {
                parameters["nonce"] = Convert.ToString(GetNonce());

                resultData = await PrivateQuery(url, parameters, token);
                BitFinexResponse response = null;

                try
                {
                    response = JsonConvert.DeserializeObject<BitFinexResponse>(resultData);
                }
                catch
                {
                    // ignored
                }

                if (response != null && !response.IsSuccess)
                {
                   // Log.Error($"RequestCategory - {requestCategory} ExchangeName - {ExchangeName.BitFinex}, Response - {resultData}");
                }

                //FileStorageHelper.StoreFile(ExchangeName.BitFinex, url, ConvertorHelper.DictionaryToJson(parameters), resultData, requestCategory, Log);

                return JsonConvert.DeserializeObject<T>(resultData);
            }
            catch (Exception ex)
            {
                if (resultData.Contains(NonceError))
                {
                    parameters["nonce"] = Convert.ToString(GetNonce());
                }
                else
                {
                    if (!(string.IsNullOrEmpty(resultData) && url.Contains("balances")))
                        ;
                        //Log.Warn("BitFinex: Can't parse json {0} to BitFinex<{1}>, URL - {2}, Exception Message - {3}, Nonce - {4}", resultData, typeof(T), url, ex.Message, parameters["nonce"]);
                }
                throw;
            }
        }

        private async Task<string> PrivateQuery(string url, Dictionary<string, object> parameters, CancellationToken token = default(CancellationToken))
        {
            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);

                client.DefaultRequestHeaders.Add("X-BFX-APIKEY", _key);
                client.DefaultRequestHeaders.Add("X-BFX-PAYLOAD", DictionaryToJsonToBase64(parameters));
                client.DefaultRequestHeaders.Add("X-BFX-SIGNATURE", Sign(DictionaryToJsonToBase64(parameters), _secret));

                using (var queryString = new StringContent(JsonConvert.SerializeObject(parameters, Formatting.None)))
                {
                    using (HttpResponseMessage response = await client.PostAsync(new Uri(url), queryString, token))
                    {
                        // Read response asynchronously 
                        return await response.Content.ReadAsStringAsync();
                    }
                }
            }
        }
        #endregion

        #region Private Methods

        public async Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var accountHoldings = await GetWalletBalances(token);

                return accountHoldings.Where(x => x.Type == "exchange").Select(
                    ah =>
                        new AccountChange(ExchangeName.BitFinex,
                            ah.Currency,
                            ah.Available)).ToArray(); // there is also 'balance' available
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<IEnumerable<BitFinexOrderStatus>> GetActiveOrders(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var methods = $"v{Version}/orders";
                    var data = await PrivateQuery<List<BitFinexOrderStatus>>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                       { "request", "/" + methods },
                       { "nonce", Convert.ToString(GetNonce()) }
                    }, token, RequestCategory.OpenOrders);

                    return data;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }


        public async Task<IEnumerable<BitFinexWalletBalance>> GetWalletBalances(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var methods = $"v{Version}/balances";

                    return await PrivateQuery<List<BitFinexWalletBalance>>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                       { "request", "/" + methods },
                       { "nonce", Convert.ToString(GetNonce()) }
                    }, token, RequestCategory.AccountHoldings);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<List<BitFinexAccountInfo>> GetAccountInfos(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var methods = $"v{Version}/account_infos";

                    var data = await PrivateQuery<List<BitFinexAccountInfo>>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                       { "request", "/" + methods },
                       { "nonce", Convert.ToString(GetNonce()) }
                    }, token);

                    return data;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<List<BitFinexMargin>> GetMarginInfos(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var methods = $"v{Version}/margin_infos";
                    var data = await PrivateQuery<List<BitFinexMargin>>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                       { "request", "/" + methods },
                       { "nonce", Convert.ToString(GetNonce()) }
                    }, token);

                    return data;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<BitFinexOrderStatus> NewOrder(Order order, BitFinexOrderSide side, string type, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    if (order == null)
                        throw new ArgumentNullException(nameof(order), "BitFinexApi NewOrder method.");

                    if (string.IsNullOrEmpty(type))
                        throw new ArgumentNullException(nameof(type), "BitFinexApi NewOrder method.");

                    var culture = CultureHelper.GetEnglishCulture();
                    var methods = $"v{Version}/order/new";

                    var correctPair = PairConvertorHelper.DashToDrkPair(order.Pair);

                    var result = await PrivateQuery<BitFinexOrderStatus>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                        { "request", "/" + methods },
                        { "nonce", Convert.ToString(GetNonce()) },
                        { "symbol", PairToString(correctPair) },
                        { "amount", Convert.ToString(order.Amount, culture.NumberFormat) },
                        { "price", Convert.ToString(order.Price, culture.NumberFormat) },
                        { "exchange", "bitfinex" },
                        { "side", side.ToString().ToLower() }, //sell, buy
                        { "type", $"exchange {type.ToLower()}"}
                    }, token, RequestCategory.SubmitOrder);

                    return result;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<BitFinexOrderStatus> CancelOrder(int orderId, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var methods = $"v{Version}/order/cancel";

                    var result = await PrivateQuery<BitFinexOrderStatus>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                        { "request", "/" + methods },
                        { "nonce", Convert.ToString(GetNonce()) },
                        { "order_id", orderId }
                    }, token, RequestCategory.CancelOrder);

                    return result;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<BitFinexResponse> CancelAllOrder(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var methods = $"v{Version}/order/cancel/all";

                    var result = await PrivateQuery<BitFinexResponse>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                        { "request", "/" + methods },
                        { "nonce", Convert.ToString(GetNonce()) }
                    }, token, RequestCategory.CancelAllOrder);

                    return result;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<BitFinexOrderStatus> GetOrderStatus(string orderId, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    if (string.IsNullOrEmpty(orderId))
                        throw new ArgumentNullException(nameof(orderId), "BitFinexApi GetOrderStatus method.");

                    var methods = $"v{Version}/order/status";

                    var result = await PrivateQuery<BitFinexOrderStatus>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                        { "request", "/" + methods },
                        { "nonce", Convert.ToString(GetNonce()) },
                        { "order_id", Convert.ToUInt32(orderId) }
                    }, token, RequestCategory.OrderStatus);

                    return result;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<BitFinixPosition>> GetPositions(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var methods = $"v{Version}/positions";

                    var result = await PrivateQuery<List<BitFinixPosition>>(_baseUri.AbsoluteUri + methods, new Dictionary<string, object>
                    {
                        { "request", "/" + methods },
                        { "nonce", Convert.ToString(GetNonce()) }
                    }, token);

                    return result;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        #endregion

        #region Buy/Sell Methods
        public async Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BitFinexApi SellMarket method.");

                if (order.MarketSide != MarketSide.Ask)
                    throw new BitFinexException("BitFinex API, Incorrect market side: {0}".FormatAs(order));

                var neworder = await NewOrder(order, BitFinexOrderSide.Sell, BitFinexOrderType.Market, token);

                return neworder.OrderId > 0 ? Convert.ToString(neworder.OrderId) : null;

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BitFinexApi BuyMarket method.");

                if (order.MarketSide != MarketSide.Bid)
                    throw new BitFinexException("BitFinex API, Incorrect market side: {0}".FormatAs(order));

                var neworder = await NewOrder(order, BitFinexOrderSide.Buy, BitFinexOrderType.Market, token);

                return neworder.OrderId > 0 ? Convert.ToString(neworder.OrderId) : null;

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BitFinexApi SellLimit method.");

                if (order.MarketSide != MarketSide.Ask)
                    throw new BitFinexException("BitFinex API, Incorrect market side: {0}".FormatAs(order));

                var neworder = await NewOrder(order, BitFinexOrderSide.Sell, BitFinexOrderType.Limit, token);

                return neworder.OrderId > 0 ? Convert.ToString(neworder.OrderId) : null;

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BitFinexApi BuyLimit method.");

                if (order.MarketSide != MarketSide.Bid)
                    throw new BitFinexException("BitFinex API, Incorrect market side: {0}".FormatAs(order));

                var neworder = await NewOrder(order, BitFinexOrderSide.Buy, BitFinexOrderType.Limit, token);

                return neworder.OrderId > 0 ? Convert.ToString(neworder.OrderId) : null;

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "BitFinexApi GetOrderStatus method.");

                if (!string.IsNullOrEmpty(order.Id))
                {
                    var bitFinexOrder = await GetOrderStatus(order.Id, token);

                    if (!bitFinexOrder.IsLive && bitFinexOrder.RemainingAmount == 0.0m)
                        return new OrderChange(order, bitFinexOrder.OriginalAmount, bitFinexOrder.AvgExecutionPrice,
                            OrderStatus.Filled, UnixTimeStampToDateTime(bitFinexOrder.Timestamp) ?? DateTime.UtcNow,
                            bitFinexOrder.OriginalAmount);

                    if (bitFinexOrder.RemainingAmount > 0.0m && bitFinexOrder.ExecutedAmount > 0.0m)
                        return new OrderChange(order, 0, bitFinexOrder.AvgExecutionPrice, OrderStatus.PartiallyFilled,
                            UnixTimeStampToDateTime(bitFinexOrder.Timestamp) ?? DateTime.UtcNow,
                            bitFinexOrder.ExecutedAmount);
                }
                return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var bitFinexOrder = await GetOrderStatus(order.Id, token);
                return Tuple.Create(bitFinexOrder.AvgExecutionPrice, bitFinexOrder.ExecutedAmount);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public OrderBook FromBitFinexOrderBook(Pair pair, BitFinexOrderBook bOb)
        {
            return new OrderBook(
                bOb.Bids.Where(x => x.Price > 0 && x.Amount > 0).Select(b => FromBitFinexOrder(pair, MarketSide.Bid, b)),
                bOb.Asks.Where(x => x.Price > 0 && x.Amount > 0).Select(b => FromBitFinexOrder(pair, MarketSide.Ask, b)),
                ExchangeName.BitFinex,
                pair,
                DateTime.UtcNow);
        }

        public Order FromBitFinexOrder(Pair pair, MarketSide marketSide, BitFinixOrder b)
        {
            return new Order(
                pair,
                b.Price,
                b.Amount,
                ExchangeName.BitFinex,
                marketSide,
                DateTime.UtcNow,
                OrderType.Limit,
                SourceSystemCode.ExternalExchange);
        }
        #endregion

        #region Public Methods

        public async Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await GetPairs(token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<List<Pair>> GetPairs(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JArray jArray = await JObjectQuery(_baseUri.AbsoluteUri + $"v{Version}/symbols", token);

                return jArray.Select(x =>
                {
                    var data = x.ToString().ToChunks(3);

                    if (data.Length > 1)
                    {
                        var supported = SupportedCurrencyHelper.GetSupportedCurrencies();

                        var inCurrency = supported.FirstOrDefault(curency => curency == data[0].ToUpper());
                        var outCurrency = supported.FirstOrDefault(curency => curency == data[1].ToUpper());

                        if (inCurrency != null && outCurrency != null)
                        {
                            return new Pair(inCurrency, outCurrency);
                        }
                    }

                    return null;

                }).Where(x => x != null).ToList();

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<BitFinexPairDetails>> GetPairsDetails(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<List<BitFinexPairDetails>>(_baseUri.AbsoluteUri +
                                                                                                $"v{Version}/symbols_details", token),
                TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        private async Task<BitFinexOrderBook> GetBitFinexOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<BitFinexOrderBook>(_baseUri.AbsoluteUri +
                                                                                        $"v{Version}/book/{PairToString(pair)}", token),
                TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (pair == null)
                    throw new ArgumentNullException(nameof(pair), "BitFinexApi GetOrderBook method.");

                var orderBook = await GetBitFinexOrderBook(pair, token);

                return FromBitFinexOrderBook(pair, orderBook);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<BitFinexTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (pair == null)
                    throw new ArgumentNullException(nameof(pair), "BitFinexApi GetTicker method.");

                return await Query<BitFinexTicker>(_baseUri.AbsoluteUri + $"v{Version}/pubticker/{PairToString(pair)}", token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<BitFinexStats>> GetStats(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (pair == null)
                    throw new ArgumentNullException(nameof(pair), "BitFinexApi GetStats method.");

                return await Query<List<BitFinexStats>>(_baseUri.AbsoluteUri + $"v{Version}/stats/{PairToString(pair)}", token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<BitFinexLandBook> GetLandBook(string currency, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<BitFinexLandBook>(_baseUri.AbsoluteUri +
                                                                                       $"v{Version}/lendbook/{currency}", token),
                TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<BitFinexTrade>> GetTrades(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (pair == null)
                    throw new ArgumentNullException(nameof(pair), "BitFinexApi GetTrades method.");

                return await Query<List<BitFinexTrade>>(_baseUri.AbsoluteUri + $"v{Version}/trades/{PairToString(pair)}", token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<BitFinexLend>> GetLends(string currency, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () => await Query<List<BitFinexLend>>(_baseUri.AbsoluteUri +
                                                                                         $"v{Version}/lends/{currency}", token), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #endregion

        #region Utils
        private static long _lastNonce;

        private static long GetNonce()
        {
            var currentNonce = DateTime.UtcNow.AddDays(1).Ticks;
            _lastNonce = currentNonce = currentNonce > _lastNonce ? currentNonce : (currentNonce + Math.Abs(_lastNonce - currentNonce) + 1);
            return currentNonce;
        }

        private string Sign(string payload, string secretKey)
        {
            var data = BitConverter.ToString(SignHMacSha384(secretKey, Encoding.UTF8.GetBytes(payload)));
            return data.Replace("-", "").ToLower();
        }

        private static byte[] SignHMacSha384(string key, byte[] data)
        {
            var hashMaker = new HMACSHA384(Encoding.ASCII.GetBytes(key));
            return hashMaker.ComputeHash(data);
        }

        private static string DictionaryToJsonToBase64(Dictionary<string, object> parameters)
        {
            string json = JsonConvert.SerializeObject(parameters, Formatting.None);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        private static string PairToString(Pair pair)
        {
            return $"{pair.BaseCurrency}{pair.CounterCurrency}".ToLower();
        }

        private static DateTime? UnixTimeStampToDateTime(string unixTimeStamp)
        {
            double timeStamp;
            if (double.TryParse(unixTimeStamp, out timeStamp))
            {
                return UnixTimeStampToDateTime(timeStamp);
            }
            return null;
        }
        private static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        #endregion
    }
}
