using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Bittrex.Model;
using Newtonsoft.Json;

namespace Exchange.Bittrex
{
    public class BittrexApi : ApiBase, IBittrexApi
    {
        private const string BaseUri = "https://bittrex.com";
        private readonly CultureInfo _culture;
        private readonly string _key;
        private readonly string _secret;

        #region Constructor
        public BittrexApi(string apiKey, string apiSecret)
        {
            _key = apiKey;
            _secret = apiSecret;
            _culture = CultureHelper.GetEnglishCulture();
        }
        #endregion

        #region PrivateAPI
        public async Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var accountHoldings = await Query<IEnumerable<BittrexAccountBalanceRecord>>("/account/getbalances", null, true, token, RequestCategory.AccountHoldings);

                    return accountHoldings.Select(ah =>
                                new AccountChange(ExchangeName.Bittrex,
                                    ah.Currency,
                                    ah.Available)); // there is also 'balance' available
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
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
                    if (order.MarketSide != MarketSide.Bid)
                        throw new ApplicationException("Bittrex API, Incorrect market side: {0}".FormatAs(order));

                    var result = await Query<BittrexResult>("/market/buylimit",
                        new Dictionary<string, string>
                    {
                        {"market",ToBittrexPair(order.Pair)},
                        {"quantity",Convert.ToString(order.Amount, _culture.NumberFormat)},
                        {"rate",Convert.ToString(order.Price, _culture.NumberFormat)}
                    },
                    true, token, RequestCategory.SubmitOrder);

                    return result.Uuid;

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal(); 

                return await RetryHelper.DoAsync(async () =>
                {
                    if (order.MarketSide != MarketSide.Ask)
                        throw new ApplicationException("Bittrex API, Incorrect market side: {0}".FormatAs(order));

                    var result = await Query<BittrexResult>("/market/selllimit",
                        new Dictionary<string, string>
                    {
                        {"market",ToBittrexPair(order.Pair)},
                        {"quantity",Convert.ToString(order.Amount, _culture.NumberFormat)},
                        {"rate",Convert.ToString(order.Price, _culture.NumberFormat)}
                    }, true, token, RequestCategory.SubmitOrder);

                    return result.Uuid;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    if (order.MarketSide != MarketSide.Bid)
                        throw new ApplicationException("Bittrex API, Incorrect market side: {0}".FormatAs(order));

                    var result = await Query<BittrexResult>("/market/buymarket",
                        new Dictionary<string, string>
                    {
                        {"market",ToBittrexPair(order.Pair)},
                        {"quantity",Convert.ToString(order.Amount, _culture.NumberFormat)}
                    }, true, token, RequestCategory.SubmitOrder);
                    return result.Uuid;

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    if (order.MarketSide != MarketSide.Ask)
                        throw new ApplicationException("Bittrex API, Incorrect market side: {0}".FormatAs(order));

                    var result = await Query<BittrexResult>("/market/sellmarket",
                        new Dictionary<string, string>
                    {
                        {"market",ToBittrexPair(order.Pair)},
                        {"quantity",Convert.ToString(order.Amount, _culture.NumberFormat)}
                    }, true, token, RequestCategory.SubmitOrder);
                    return result.Uuid;

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<bool> CancelOrder(string uuid, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var result = await Query<BittrexBaseResult>("/market/cancel",
                    new Dictionary<string, string>
                    {
                        {"uuid",uuid},
                    },
                    true, token, RequestCategory.CancelOrder);

                    return result.Success;

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<BittrexGetOrderRecord> GetOrder(string uuid, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () => await Query<BittrexGetOrderRecord>("/account/getorder",
                   new Dictionary<string, string>
                    {
                        {"uuid",uuid},
                    }, true, token, RequestCategory.OrderStatus), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<BittrexGetOpenOrdersRecord>> GetAllOpenOrders(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () => await Query<List<BittrexGetOpenOrdersRecord>>("/market/getopenorders",
                   null, true, token, RequestCategory.OpenOrders), TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        #endregion

        #region Public API
        /// <summary>
        /// Unfortunately, Bittrex supports only first 50 records of the orderbook
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var orderBook = await Query<BittrexOrderBook>(
                                "/public/getorderbook",
                                new Dictionary<string, string> { { "market", ToBittrexPair(pair) }, { "type", "both" }, { "depth", "50" } }
                                , false, token);

                return FromBittrexOrderBook(pair, orderBook);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }


        public async Task<IEnumerable<BittrexMarket>> GetMarkets(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query<IEnumerable<BittrexMarket>>("/public/getmarkets", null, true, token);
                return result;

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<BittrexMarketSummary>> GetMarketSummaries(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query<IEnumerable<BittrexMarketSummary>>("/public/getmarketsummaries", null, true, token);
                return result;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<BittrexTicker> GetTicker(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query<BittrexTicker>("/public/getticker",
                new Dictionary<string, string> { { "market", ToBittrexPair(pair) } }
                , true, token);
                return result;

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #endregion Public API

        #region Query

        public async Task<T> Query<T>(string uri, Dictionary<string, string> args = null, bool authenticationRequired = false, CancellationToken token = default(CancellationToken), RequestCategory requestCategory = RequestCategory.Ignore)
        {
            var responseStr = await Query(uri, args, authenticationRequired, token);
            BittrexSingleResponse<T> responseObject;

            try
            {
                //FileStorageHelper.StoreFile(ExchangeName.Bittrex, uri, ConvertorHelper.DictionaryToJson(args), responseStr, requestCategory, Log);
                responseObject = JsonConvert.DeserializeObject<BittrexSingleResponse<T>>(responseStr);
            }
            catch (Exception ex)
            {
                //Log.Warn("Bittrex: Can't parse json {0} to BittrexSingleResponse<{1}>, Message - {2}", responseStr, typeof(T), ex.Message);
                throw;
            }

            if (!responseObject.success)
            {
                throw new ApplicationException("Bittrex API Exception:" + responseObject.message);
            }

            return responseObject.result;
        }

        public async Task<string> Query(string uri, Dictionary<string, string> args = null, bool authenticationRequired = false, CancellationToken token = default(CancellationToken))
        {
            string queryStr = String.Empty;

            if (authenticationRequired)
            {
                queryStr = EncodeParameters(new Dictionary<string, string>{ { "apikey", _key }, { "nonce", GetNonce().ToString("D") } });
            }

            queryStr += (args == null) ? String.Empty : "&" + EncodeParameters(args);
            uri = "/api/v1.1" + uri + "?" + queryStr;

            var baseUri = new Uri(BaseUri);
            var relativeUri = new Uri(uri, UriKind.Relative);
            var fullUri = new Uri(baseUri, relativeUri);

            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds( Constant.TimeOut);

                if (authenticationRequired)
                {
                    client.DefaultRequestHeaders.Add("apisign", Sign(fullUri.AbsoluteUri, _secret));
                }

                using (var response = await client.GetAsync(fullUri, token))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
            
        }

        #endregion

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var bittrexOrder = await GetOrder(order.Id, token);
                return Tuple.Create(bittrexOrder.Price ?? -1, bittrexOrder.QuantityRemaining == 0 ? order.Amount : bittrexOrder.Quantity != null && bittrexOrder.QuantityRemaining != null ?
                        bittrexOrder.Quantity.Value - bittrexOrder.QuantityRemaining.Value : -1);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (!string.IsNullOrEmpty(order.Id))
                {
                    var bittrexOrder = await GetOrder(order.Id, token);

                    if (bittrexOrder.QuantityRemaining == 0)
                        return new OrderChange(order, order.Amount, bittrexOrder.Price ?? 0, OrderStatus.Filled, bittrexOrder.Closed ?? DateTime.UtcNow, order.Amount);

                    if (bittrexOrder.QuantityRemaining > 0)
                    {
                        return new OrderChange(order, 0, bittrexOrder.Price ?? 0, OrderStatus.PartiallyFilled, bittrexOrder.Closed ?? DateTime.UtcNow,
                            bittrexOrder.Quantity != null && bittrexOrder.QuantityRemaining != null ?
                            bittrexOrder.Quantity.Value - bittrexOrder.QuantityRemaining.Value : 0);
                    }
                }
               
                return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        private OrderBook FromBittrexOrderBook(Pair pair, BittrexOrderBook bOb)
        {
            return new OrderBook(
                bOb.buy.Select(b => FromBittrexOrder(pair, MarketSide.Bid, b)),
                bOb.sell.Select(b => FromBittrexOrder(pair, MarketSide.Ask, b)),
                ExchangeName.Bittrex,
                pair,
                DateTime.UtcNow);
        }

        private Order FromBittrexOrder(Pair pair, MarketSide marketSide, BittrexOrder b)
        {
            return new Order(
                pair,
                b.Rate,
                b.Quantity,
                ExchangeName.Bittrex,
                marketSide,
                DateTime.UtcNow,
                OrderType.Limit,
                SourceSystemCode.ExternalExchange);
        }

        #region Util methods
        private static long _lastNonce = 0;

        private long GetNonce()
        {
            var currentNonce = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).Ticks;
            _lastNonce = currentNonce = currentNonce > _lastNonce ? currentNonce : (currentNonce + Math.Abs(_lastNonce - currentNonce) + 1);
            return currentNonce;
        }

        private string Sign(string uri, string secretKey)
        {
            return ByteArrayToString(
                SignHmacSha512(secretKey, StringToByteArray(uri)))
                    .ToUpper();
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
        private static byte[] SignHmacSha512(string key, byte[] data)
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
        private static byte[] StringToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }

        private string ToBittrexPair(Pair pair)
        {
            return "{0}-{1}".FormatAs(pair.BaseCurrency, pair.CounterCurrency);
        }
        #endregion Util methods
    }
}
