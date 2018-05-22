using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Kraken.Helper;
using Exchange.Kraken.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.Kraken.Exceptions;

namespace Exchange.Kraken
{
    public class KrakenApi : IKrakenApi
    {
        private const string BaseUri = "https://api.kraken.com/";
        public bool IsRestartAfterTime { get; set; }
        private string _apiKey;
        private string _apiSecret;

        public KrakenApi(string apiKey, string apiSecret)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["KrakenMultiplier"]))
            {
                long.TryParse(ConfigurationManager.AppSettings["KrakenMultiplier"], out _multiplier);
            }

            _multiplier = _multiplier <= 0 ? 1 : _multiplier;
        }

        #region Public Api methods
        /// <summary>
        /// Get server time
        /// </summary>
        public async Task<DateTime> GetTime(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject o = await Query("0/public/Time", null, token);
                return KrakenTime.ReadFromJObject(o).Time;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        /// <summary>
        /// Get asset info
        /// </summary>
        public async Task<KrakenAssets> GetAssets(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject o = await Query("0/public/Assets", null, token);
                return KrakenAssets.ReadFromJObject(o);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        /// <summary>
        /// Get tradable asset pairs
        /// </summary>
        public async Task<KrakenAssetPairs> GetAssetPairs(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                JObject o = await Query("0/public/AssetPairs", null, token);
                return KrakenAssetPairs.ReadFromJObject(o);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        /// <summary>
        /// Get ticker information
        /// </summary>
        public async Task<KrakenTicket> GetTicker(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                string krakenPair = ToKrakenPair(pair).ConvertToString();
                JObject o = await Query("0/public/Ticker", new Dictionary<string, string>
                {
                     {"pair", krakenPair}
                }, token);
                return (o[krakenPair]).ToObject<KrakenTicket>();
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        /// <summary>
        /// Get OHLC data
        /// </summary>
        public async Task<KrakenData> GetOhlc(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                string krakenPair = ToKrakenPair(pair).ConvertToString();
                JObject o = await Query("0/public/OHLC", new Dictionary<string, string>
                {
                   {"pair", krakenPair}
                }, token);
                var data = new KrakenData
                {
                    Last = (o["last"]).ToObject<long>(),
                    DataList = (o[krakenPair]).ToObject<List<string[]>>()
                };
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        /// <summary>
        /// Get order book
        /// </summary>
        public async Task<OrderBook> GetOrderBook(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                string krakenPair = ToKrakenPair(pair).ConvertToString();
                JObject o = await Query("0/public/Depth", new Dictionary<string, string>
                {
                   {"pair", krakenPair}
                }, token);
                var orderBook = (o[krakenPair]).ToObject<KrakenOrderBook>();
                return FromKrakenOrderBook(orderBook, pair);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        /// <summary>
        /// Get recent trades
        /// </summary>
        public async Task<KrakenData> GetTrades(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                string krakenPair = ToKrakenPair(pair).ConvertToString();
                JObject o = await Query("0/public/Trades", new Dictionary<string, string>
                {
                   { "pair", krakenPair }
                }, token);
                var data = new KrakenData
                {
                    Last = (o["last"]).ToObject<long>(),
                    DataList = (o[krakenPair]).ToObject<List<string[]>>()
                };
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        /// <summary>
        /// Get recent spread data
        /// </summary>
        public async Task<KrakenData> GetSpread(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                string krakenPair = ToKrakenPair(pair).ConvertToString();
                JObject o = await Query("0/public/Spread", new Dictionary<string, string>
                {
                   { "pair", krakenPair }
                }, token);
                var data = new KrakenData
                {
                    Last = (o["last"]).ToObject<long>(),
                    DataList = (o[krakenPair]).ToObject<List<string[]>>()
                };
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<IEnumerable<Pair>> GetSupportedPairs(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                return await Task<IEnumerable<Pair>>.Factory.StartNew(() =>
                {
                    var resultList = new List<Pair>();
                    var data = Enum.GetValues(typeof(KrakenPair));
                    data.OfType<KrakenPair>().ToList().ForEach(x => resultList.Add(FromKrakenPair(x)));
                    return resultList;
                }, token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        #endregion

        #region PrivateAPI

        public async Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                try
                {
                    JObject result = await QueryPrivate("Balance", null, token, RequestCategory.AccountHoldings);
                    var accountHoldings = result.ToObject<Dictionary<string, decimal>>();
                    var supportedCurrency = SupportedCurrencyHelper.GetSupportedCurrencies();
                    return accountHoldings.Where(x => supportedCurrency.Contains(String.Join(String.Empty, x.Key.Skip(1))))
                        .Select(ah => new AccountChange(ExchangeName.Kraken, FromKrakenCurrency(ah.Key), ah.Value)).ToArray();
                }
                catch (KrakenInvalidOrderException ex)
                {
                    //_log.Error(ex.Message, ex);
                }
                catch (KrakenRateLimitExceededException ex)
                {
                    IsRestartAfterTime = true;
                    //_log.Error(ex.Message, ex);
                }
                catch (KrakenInvalidKeyException ex)
                {
                    //_log.Error(ex.Message, ex);
                }
                catch (KrakenTemporaryLockOutException ex)
                {
                    IsRestartAfterTime = true;
                    //_log.Error(ex.Message, ex);
                }
                catch (KrakenInvalidNonceException ex)
                {
                    //_log.Error(ex.Message, ex);
                }
                catch (KrakenInsufficientFundsException ex)
                {
                    //_log.Error(ex.Message, ex);
                }

                return null;
            }, TimeSpan.FromMilliseconds(Constant.KrakenRetryInterval));
        }

        public Task<JObject> GetTradeBalance(string aclass, string asset, CancellationToken token = default(CancellationToken))
        {
            return RetryHelper.WebDoAsync(async () =>
            {
                string reqs = String.Empty;

                if (string.IsNullOrEmpty(aclass))
                {
                    reqs += string.Format("&aclass={0}", aclass);
                }
                if (string.IsNullOrEmpty(aclass))
                {
                    reqs += string.Format("&asset={0}", asset);
                }

                return await QueryPrivate("TradeBalance", reqs, token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval),1);
        }

        public async Task<string> BuyLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                if (order.MarketSide != MarketSide.Bid)
                    throw new ApplicationException(string.Format("Kraken API, Incorrect market side: {0}", order));

                var krakenOrder = new KrakenOrder
                {
                    Type = "buy",
                    OrderType = KrakenOrderType.Limit.ToString().ToLower(),
                    Pair = ToKrakenPair(order.Pair).ToString(),
                    Price = order.Price,
                    Volume = order.Amount
                };

                try
                {
                    var result = await AddOrder(krakenOrder, token);
                    var items = (JArray)result["txid"];
                    return items.First.ToString();
                }
                catch (KrakenInvalidOrderException)
                {
                    return null;
                }
                catch (KrakenRateLimitExceededException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidKeyException)
                {
                    return null;
                }
                catch (KrakenTemporaryLockOutException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidNonceException)
                {
                    return null;
                }
                catch (KrakenInsufficientFundsException)
                {
                    return null;
                }

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> SellLimit(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                if (order.MarketSide != MarketSide.Ask)
                    throw new ApplicationException(string.Format("Kraken API, Incorrect market side: {0}", order));

                var krakenOrder = new KrakenOrder
                {
                    Type = "sell",
                    OrderType = KrakenOrderType.Limit.ToString().ToLower(),
                    Pair = ToKrakenPair(order.Pair).ToString(),
                    Price = order.Price,
                    Volume = order.Amount
                };

                try
                {
                    var result = await AddOrder(krakenOrder, token);
                    var items = (JArray)result["txid"];
                    return items.First.ToString();
                }
                catch (KrakenInvalidOrderException)
                {
                    return null;
                }
                catch (KrakenRateLimitExceededException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidKeyException)
                {
                    return null;
                }
                catch (KrakenTemporaryLockOutException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidNonceException)
                {
                    return null;
                }
                catch (KrakenInsufficientFundsException)
                {
                    return null;
                }

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> BuyMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                if (order.MarketSide != MarketSide.Bid)
                    throw new ApplicationException(string.Format("Kraken API, Incorrect market side: {0}", order));

                var krakenOrder = new KrakenOrder
                {
                    Type = "buy",
                    OrderType = KrakenOrderType.Market.ToString().ToLower(),
                    Pair = ToKrakenPair(order.Pair).ToString(),
                    Price = order.Price,
                    Volume = order.Amount
                };

                try
                {
                    var result = await AddOrder(krakenOrder, token);
                    var items = (JArray)result["txid"];
                    return items.First.ToString();
                }
                catch (KrakenInvalidOrderException)
                {
                    return null;
                }
                catch (KrakenRateLimitExceededException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidKeyException)
                {
                    return null;
                }
                catch (KrakenTemporaryLockOutException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidNonceException)
                {
                    return null;
                }
                catch (KrakenInsufficientFundsException)
                {
                    return null;
                }

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> SellMarket(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                if (order.MarketSide != MarketSide.Ask)
                    throw new ApplicationException(string.Format("Kraken API, Incorrect market side: {0}", order));

                var krakenOrder = new KrakenOrder
                {
                    Type = "sell",
                    OrderType = KrakenOrderType.Market.ToString().ToLower(),
                    Pair = ToKrakenPair(order.Pair).ToString(),
                    Price = order.Price,
                    Volume = order.Amount
                };

                try
                {
                    var result = await AddOrder(krakenOrder, token);
                    var items = (JArray)result["txid"];
                    return items.First.ToString();
                }
                catch (KrakenInvalidOrderException)
                {
                    return null;
                }
                catch (KrakenRateLimitExceededException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidKeyException)
                {
                    return null;
                }
                catch (KrakenTemporaryLockOutException)
                {
                    IsRestartAfterTime = true;
                    return null;
                }
                catch (KrakenInvalidNonceException)
                {
                    return null;
                }
                catch (KrakenInsufficientFundsException)
                {
                    return null;
                }

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<int> CancelOrder(string txid, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                string reqs = string.Format("&txid={0}", txid);
                JObject jObj = await QueryPrivate("CancelOrder", reqs, token, RequestCategory.CancelOrder);
                var count = jObj["count"].ToObject<int>();
                return count;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<KrakenOrder> GetOrder(string txid, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                string reqs = string.Format("&txid={0}", txid);
                JObject o = await QueryPrivate("QueryOrders", reqs, token, RequestCategory.OrderStatus);
                var record = (o[txid]).ToObject<KrakenGetOrderRecord>();
                return new KrakenOrder(record);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        private async Task<KrakenGetOrderRecord> GetOrderStatus(string txid, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                string reqs = string.Format("&txid={0}", txid);
                JObject o = await QueryPrivate("QueryOrders", reqs, token, RequestCategory.OrderStatus);
                var record = (o[txid]).ToObject<KrakenGetOrderRecord>();
                return record;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<KrakenOrder>> GetOpenOrders(bool trades = false, string userref = "", CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                string reqs = string.Format("&trades={0}", true);

                if (!String.IsNullOrEmpty(userref))
                    reqs += string.Format("&userref={0}", userref);
                JObject o = await QueryPrivate("OpenOrders", reqs, token, RequestCategory.OpenOrders);
                var records = o["open"].OfType<JProperty>()
                            .Select(x => new KrakenOrder
                            {
                                TxId = x.Name
                            }).ToList();
                return records;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                var krakenOrder = await GetOrderStatus(order.Id, token);
                return Tuple.Create(krakenOrder.Price, krakenOrder.VolExec);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.WebDoAsync(async () =>
            {
                if (!String.IsNullOrEmpty(order.Id))
                {
                    var krakenOrder = await GetOrder(order.Id, token);

                    var avgData = await GetAvgPriceAndTotalFilledAmount(order, token);

                    if (krakenOrder.Status == KrakenOrderStatus.Closed)
                        return new OrderChange(order, order.Amount, avgData.Item1, OrderStatus.Filled, DateTime.UtcNow, order.Amount);

                    if (krakenOrder.Status == KrakenOrderStatus.Expired)
                        return new OrderChange(order, 0, avgData.Item1, OrderStatus.Expired, DateTime.UtcNow, avgData.Item2);
                }

                return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        private async Task<JObject> AddOrder(string pair,
            string type,
            string ordertype,
            decimal volume,
            decimal? price,
            decimal? price2,
            string leverage = "none",
            string position = "",
            string oflags = "",
            string starttm = "",
            string expiretm = "",
            string userref = "",
            bool validate = false,
            Dictionary<string, string> close = null,
            CancellationToken token = default(CancellationToken))
        {
            string reqs = string.Format("&pair={0}&type={1}&ordertype={2}&volume={3}&leverage={4}", pair, type, ordertype, volume, leverage);
            if (price.HasValue)
                reqs += string.Format("&price={0}", price.Value);
            if (price2.HasValue)
                reqs += string.Format("&price2={0}", price2.Value);
            if (!string.IsNullOrEmpty(position))
                reqs += string.Format("&position={0}", position);
            if (!string.IsNullOrEmpty(starttm))
                reqs += string.Format("&starttm={0}", starttm);
            if (!string.IsNullOrEmpty(expiretm))
                reqs += string.Format("&expiretm={0}", expiretm);
            if (!string.IsNullOrEmpty(oflags))
                reqs += string.Format("&oflags={0}", oflags);
            if (!string.IsNullOrEmpty(userref))
                reqs += string.Format("&userref={0}", userref);
            if (validate)
                reqs += "&validate=true";
            if (close != null)
            {
                string closeString = string.Format("&close[ordertype]={0}&close[price]={1}&close[price2]={2}", close["ordertype"], close["price"], close["price2"]);
                reqs += closeString;
            }
            return await QueryPrivate("AddOrder", reqs, token, RequestCategory.SubmitOrder);
        }

        public async Task<JObject> AddOrder(KrakenOrder krakenOrder, CancellationToken token = default(CancellationToken))
        {
            return await AddOrder(pair: krakenOrder.Pair,
                type: krakenOrder.Type,
                ordertype: krakenOrder.OrderType,
                volume: krakenOrder.Volume,
                price: krakenOrder.Price,
                price2: krakenOrder.Price2,
                leverage: krakenOrder.Leverage ?? "none",
                position: krakenOrder.Position ?? string.Empty,
                oflags: krakenOrder.OFlags ?? string.Empty,
                starttm: krakenOrder.Starttm ?? string.Empty,
                expiretm: krakenOrder.Expiretm ?? string.Empty,
                userref: krakenOrder.Userref ?? string.Empty,
                validate: krakenOrder.Validate,
                close: krakenOrder.Close,
                token: token);
        }

        #endregion

        #region Query

        private async Task<JObject> Query(string uri, Dictionary<string, string> args = null, CancellationToken token = default(CancellationToken))
        {
            KrakenResponse responseObject;
            dynamic jObject;

            var data = await PublicQuery(uri, args, token);
            try
            {
                jObject = JObject.Parse(data);

                responseObject = JsonConvert.DeserializeObject<KrakenResponse>(data);
            }
            catch (Exception)
            {
                //_log.Warn("Kraken: Can't parse json {0} to KrakenSingleResponse<{1}>", data, typeof(KrakenResponse));
                throw;
            }

            if (responseObject.Error.Any(x => x == ErrorRateLimit))
            {
                throw new KrakenRateLimitExceededException("Kraken API Exception:" + responseObject.Error.First());
            }

            if (responseObject.Error.Any())
                throw new ApplicationException("Kraken API Exception:" + responseObject.Error.First());

            return jObject.result;
        }

        private async Task<string> PublicQuery(string uri, Dictionary<string, string> args = null, CancellationToken token = default(CancellationToken))
        {
            string queryStr = String.Empty;
            queryStr += (args == null) ? String.Empty : "&" + EncodeParameters(args);
            uri = uri + "?" + queryStr.TrimStart('&');

            var fullUri = new Uri(new Uri(BaseUri), new Uri(uri, UriKind.Relative));

            return await QueryHelper.Query(fullUri.AbsoluteUri, token);
        }

        private const string ErrorRateLimit = "EAPI:Rate limit exceeded";
        private const string ErrorInvalidKey = "EAPI:Invalid key";
        private const string ErrorInvalidNonce = "EAPI:Invalid nonce";
        private const string TemporaryLockOut = "EGeneral:Temporary lockout";
        private const string ErrorInsufficientFunds = "EOrder:Insufficient funds";

        private readonly string[] _orderErrors =
        {
            "EOrder:Cannot open position",
            "EOrder:Cannot open opposing position",
            "EOrder:Margin allowance exceeded",
            "EOrder:Margin level too low",
            "EOrder:Insufficient margin",
            "EOrder:Insufficient funds",
            "EOrder:Order minimum not met",
            "EOrder:Orders limit exceeded",
            "EOrder:Positions limit exceeded",
            "EOrder:Rate limit exceeded",
            "EOrder:Scheduled orders limit exceeded",
            "EOrder:Unknown position"
        };

        private async Task<JObject> QueryPrivate(string method, 
            string postData = null, 
            CancellationToken token = default(CancellationToken),
            RequestCategory requestCategory = RequestCategory.Ignore)
        {
            KrakenResponse responseObject;
            dynamic jObj;

            var data = await PrivateQuery(method, postData, token);

            try
            {
                //FileStorageHelper.StoreFile(ExchangeName.Kraken, BaseUri + string.Format("{0}/private/{1}", Version, method), postData ?? String.Empty, data, requestCategory, _log);

                jObj = JObject.Parse(data);

                responseObject = JsonConvert.DeserializeObject<KrakenResponse>(data);

            }
            catch (Exception)
            {
                //_log.Warn("Kraken: Can't parse json {0} to KrakenSingleResponse<{1}>", data, typeof(KrakenResponse));
                throw;
            }

            if (responseObject.Error.Any(x => _orderErrors.Any(oerr => oerr==x)))
            {
                throw new KrakenInvalidOrderException("Kraken API Exception:" + responseObject.Error.First());
            }

            if (responseObject.Error.Any(x => x == ErrorRateLimit))
            {
                throw new KrakenRateLimitExceededException("Kraken API Exception:" + responseObject.Error.First());
            }

            if (responseObject.Error.Any(x => x == ErrorInvalidKey))
            {
                throw new KrakenInvalidKeyException("Kraken API Exception:" + responseObject.Error.First());
            }

            if (responseObject.Error.Any(x => x == TemporaryLockOut))
            {
                throw new KrakenTemporaryLockOutException("Kraken API Exception:" + responseObject.Error.First());
            }

            if (responseObject.Error.Any(x => x == ErrorInvalidNonce))
            {
                throw new KrakenInvalidNonceException("Kraken API Exception:" + responseObject.Error.First());
            }

            if (responseObject.Error.Any(x => x == ErrorInsufficientFunds))
            {
                throw new KrakenInsufficientFundsException("Kraken API Exception:" + responseObject.Error.First());
            }

            if (responseObject.Error.Any())
                throw new ApplicationException("Kraken API Exception:" + responseObject.Error.First());

            return jObj.result;
        }

        private const string Version = "0";

        private async Task<string> PrivateQuery(string method, string postData = null, CancellationToken token = default(CancellationToken))
        {
            
            string path = string.Format("/{0}/private/{1}", Version, method);
            string apiUrl = BaseUri + string.Format("{0}/private/{1}", Version, method);

            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);

                Int64 nonce = GetNonce();
                postData = "nonce=" + nonce + postData;
                string signature = CreateSignature(nonce, path, postData);

                client.DefaultRequestHeaders.Add("API-Key", _apiKey);
                client.DefaultRequestHeaders.Add("API-Sign", signature);

                var content = new StringContent(postData);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using (var response = await client.PostAsync(apiUrl, content, token))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        #endregion

        #region Util methods

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

        #endregion

        #region Helper methods
        private static long _multiplier = 1;
        private static long _lastNonce = 0;

        private long GetNonce()
        {
            var currentNonce = Math.Abs(DateTime.UtcNow.Ticks * _multiplier);
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

        private string CreateSignature(Int64 nonce, string path, string postData)
        {
            // generate a 64 bit nonce using a timestamp at tick resolution
            byte[] base64DecodedSecred = Convert.FromBase64String(_apiSecret);
            var np = nonce + Convert.ToChar(0) + postData;
            var pathBytes = Encoding.UTF8.GetBytes(path);
            var hash256Bytes = sha256_hash(np);
            var z = new byte[pathBytes.Count() + hash256Bytes.Count()];
            pathBytes.CopyTo(z, 0);
            hash256Bytes.CopyTo(z, pathBytes.Count());
            var signature = GetHash(base64DecodedSecred, z);

            return Convert.ToBase64String(signature);
        }

        private static byte[] sha256_hash(String value)
        {
            using (SHA256 hash = SHA256.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));
                return result;
            }
        }

        private static byte[] GetHash(byte[] keyByte, byte[] messageBytes)
        {
            using (var hmacsha512 = new HMACSHA512(keyByte))
            {
                Byte[] result = hmacsha512.ComputeHash(messageBytes);
                return result;
            }
        }

        private string FromKrakenCurrency(string currency)
        {
            switch (currency)
            {
                case "XLTC":
                    return SupportedCurrency.LTC;
                case "XNMC":
                    return SupportedCurrency.NMC;
                case "XSTR":
                    return SupportedCurrency.STR;
                case "XXBT":
                    return SupportedCurrency.BTC;
                case "XXDG":
                    return SupportedCurrency.DOGE;
                case "XXRP":
                    throw new NotSupportedException();
                case "XXVN":
                    throw new NotSupportedException();
                case "ZEUR":
                    return SupportedCurrency.EUR;
                case "ZGBP":
                    return SupportedCurrency.GBP;
                case "ZJPY":
                    return SupportedCurrency.JPY;
                case "ZKRW":
                    return SupportedCurrency.KRW;
                case "ZUSD":
                    return SupportedCurrency.USD;

                default: throw new NotSupportedException();
            }
        }

        private KrakenPair ToKrakenPair(Pair pair)
        {
            // Attention: some Kraken pair is reversed, some is right!!!
            if (pair.BaseCurrency == SupportedCurrency.LTC && pair.CounterCurrency == SupportedCurrency.EUR)
                return KrakenPair.XLTCZEUR;
            if (pair.BaseCurrency == SupportedCurrency.LTC && pair.CounterCurrency == SupportedCurrency.USD)
                return KrakenPair.XLTCZUSD;
            if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.LTC)
                return KrakenPair.XXBTXLTC;
            if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.NMC)
                return KrakenPair.XXBTXNMC;
            if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.STR)
                return KrakenPair.XXBTXSTR;
            if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.DOGE)
                return KrakenPair.XXBTXXDG;
            //XRP - Ripple Currency http://en.wikipedia.org/wiki/Ripple_(payment_protocol)
            //if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.XRP)
            //    return KrakenPair.XXBTXXRP;
            //if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.XVN)
            //    return KrakenPair.XXBTXXVN;
            if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.EUR)
                return KrakenPair.XXBTZEUR;
            if (pair.BaseCurrency == SupportedCurrency.BTC && pair.CounterCurrency == SupportedCurrency.USD)
                return KrakenPair.XXBTZUSD;
            //XVN - Ven http://en.wikipedia.org/wiki/Ven_(currency)
            //if (pair.BaseCurrency == SupportedCurrency.XVN && pair.CounterCurrency == SupportedCurrency.EUR)
            //    return KrakenPair.ZEURXXVN;
            //if (pair.BaseCurrency == SupportedCurrency.XVN && pair.CounterCurrency == SupportedCurrency.USD)
            //    return KrakenPair.ZUSDXXVN;            

            throw new NotSupportedException();
        }

        private Pair FromKrakenPair(KrakenPair pair)
        {
            switch (pair)
            {
                case KrakenPair.XLTCZEUR:
                    return new Pair(SupportedCurrency.LTC, SupportedCurrency.EUR);
                case KrakenPair.XLTCZUSD:
                    return new Pair(SupportedCurrency.LTC, SupportedCurrency.USD);
                case KrakenPair.XXBTXLTC:
                    return new Pair(SupportedCurrency.BTC, SupportedCurrency.LTC);
                case KrakenPair.XXBTXNMC:
                    return new Pair(SupportedCurrency.BTC, SupportedCurrency.NMC);
                case KrakenPair.XXBTXSTR:
                    return new Pair(SupportedCurrency.BTC, SupportedCurrency.STR);
                case KrakenPair.XXBTXXDG:
                    return new Pair(SupportedCurrency.BTC, SupportedCurrency.DOGE);
                case KrakenPair.XXBTZEUR:
                    return new Pair(SupportedCurrency.BTC, SupportedCurrency.EUR);
                case KrakenPair.XXBTZUSD:
                    return new Pair(SupportedCurrency.BTC, SupportedCurrency.USD);
                case KrakenPair.XXBTXXRP:
                    return new Pair(SupportedCurrency.XBT, SupportedCurrency.XRP);
                case KrakenPair.XXBTXXVN:
                    return new Pair(SupportedCurrency.XBT, SupportedCurrency.XVN);
                case KrakenPair.ZEURXXVN:
                    return new Pair(SupportedCurrency.EUR, SupportedCurrency.XVN);
                case KrakenPair.ZUSDXXVN:
                    return new Pair(SupportedCurrency.USD, SupportedCurrency.XVN);

                default: throw new NotSupportedException();
            }
        }

        private OrderBook FromKrakenOrderBook(KrakenOrderBook krakenOrderBook, Pair pair)
        {
            var culture = CultureHelper.GetEnglishCulture();
            var ob = new OrderBook(
                krakenOrderBook.Bids.Select(
                strOrder => new Order
                    (
                        pair,
                        Convert.ToDecimal(strOrder[0], culture.NumberFormat),
                        Convert.ToDecimal(strOrder[1], culture.NumberFormat),
                        ExchangeName.Kraken,
                        MarketSide.Bid,
                        DateTime.UtcNow,
                        OrderType.Limit,
                        SourceSystemCode.ExternalExchange
                    )).ToList(),
                krakenOrderBook.Asks.Select(
                strOrder => new Order
                    (
                        pair,
                        Convert.ToDecimal(strOrder[0], culture.NumberFormat),
                        Convert.ToDecimal(strOrder[1], culture.NumberFormat),
                        ExchangeName.Kraken,
                        MarketSide.Ask,
                        DateTime.UtcNow,
                        OrderType.Limit,
                        SourceSystemCode.ExternalExchange
                    )).ToList(),
                ExchangeName.Kraken,
                pair,
                DateTime.UtcNow);
            return ob;
        }

        #endregion
    }

}
