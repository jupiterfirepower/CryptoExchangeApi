/* Developed by Lander V
 * Buy me a beer: 1KBkk4hDUpuRKckMPG3PQj3qzcUaQUo7AB (BTC)
 * 
 * Many thanks to HaasOnline!
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Common.Contracts;
using Exchange.Cryptsy.Enums;
using Exchange.Cryptsy.Model;
using Exchange.Cryptsy.Responses;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Exchange.Cryptsy.Exceptions;

namespace Exchange.Cryptsy
{
    public class CryptsyApi : ICryptsyApi
    {
        // Private variables
        private ConcurrentBag<CryptsyMarketInfo> _marketInfos; //Save basic marketinfo, to quickly find the marketID of a given currency-pair
        private readonly HMACSHA512 _hmac = new HMACSHA512();
        private static readonly Encoding Encoding = Encoding.UTF8;

        private readonly Uri _apiUri = new Uri("https://www.cryptsy.com/api"); 
        private readonly Uri _apiFirstDataUri = new Uri("http://pubapi.cryptsy.com/api.php");
        private readonly Uri _apiSecondDataUri = new Uri("http://pubapi1.cryptsy.com/api.php");

        private string _apiKey;
        private string _apiSecret;
        private string _apiUserName;

        public CryptsyApi(string apiKey, string apiSecret, string apiUserName)
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _apiUserName = apiUserName;
            _hmac.Key = Encoding.GetBytes(apiSecret);
            LastMessage = string.Empty;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["CryptsyMultiplier"]))
            {
                BigInteger.TryParse(ConfigurationManager.AppSettings["CryptsyMultiplier"], out _multiplier);
            }
            _multiplier = _multiplier <= 0 ? 1 : _multiplier;
        }

        // Public variable
        public string LastMessage { get; private set; }

        public ConcurrentBag<CryptsyMarketInfo> MarketInfos
        {
            get { return _marketInfos; }
            set { _marketInfos = value; }
        }

        #region Public Api Methods
        /* Returns marketinfo for the market for these two currencies (order doesn't matter)
         * Returns null if no market was found with these currencies
         * If basicInfoOnly = true, recent trades and top orders will not be loaded
         */
        public async Task<CryptsyMarketInfo> GetMarketInfo(string currencyCode1, string currencyCode2, bool basicInfoOnly = false, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (_marketInfos == null)
                {
                    _marketInfos = await GetOpenMarkets(true, token);//Don't load recent trades and orderbook for all markets
                }

                currencyCode1 = currencyCode1.ToUpper();
                currencyCode2 = currencyCode2.ToUpper();
                CryptsyMarketInfo market = _marketInfos.FirstOrDefault(m => currencyCode1 == m.PrimaryCurrencyCode && currencyCode2 == m.SecondaryCurrencyCode || currencyCode2 == m.PrimaryCurrencyCode && currencyCode1 == m.SecondaryCurrencyCode);

                if (market == null)
                    return null;

                return basicInfoOnly ? market : await GetMarketInfo(market.MarketId, token); //Get all info from the requested market
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<CryptsyMarketInfo> GetMarketInfo(long marketId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var args = new Dictionary<string, string>
                {
                     { "marketid", Convert.ToString(marketId)},
                     { "method", "singlemarketdata" }
                };

                CryptsyResponse answer = await CryptsyPublicQuery(args, false, token);

                return answer.Success ? CryptsyMarketInfo.ReadMultipleFromJObject(answer.Data as JObject).SingleOrDefault() : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<ConcurrentBag<CryptsyMarketInfo>> GetOpenMarkets(bool basicInfoOnly = false, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse answer = await CryptsyPublicQuery(new Dictionary<string, string> { { "method", "marketdatav2" } }, false, token);

                return answer.Success ? CryptsyMarketInfo.ReadMultipleFromJObject(answer.Data as JObject, basicInfoOnly) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        //Set basicInfoOnly=true to skip recent trades & top 20 buy and sell orders
        public ConcurrentBag<CryptsyMarketInfo> GetOpenMarketsPeriodically(CryptsyMarket[] markets, bool basicInfoOnly = false)
        {
            var resultList = new ConcurrentBag<CryptsyMarketInfo>();

            var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount * 50 };
            int marketCounts = markets.Length;
           
            var sw = new Stopwatch();
            sw.Start();

            Parallel.For(1, marketCounts,
                options,
                () => new Tuple<CryptsyResponse>(new CryptsyResponse()),
                (i, pls, state) =>
                {
                    try
                    {
                        CryptsyResponse answer = CryptsyPublicQuery(new Dictionary<string, string> { { "method", "singlemarketdata" }, { "marketid", Convert.ToString(markets[i - 1].MarketId) } }, i % 2 == 0).Result;
                        state = new Tuple<CryptsyResponse>(answer);
                    }
                    catch
                    {
                        state = new Tuple<CryptsyResponse>(null);
                    }
                    return state;
                },
                state => 
                {
                    if (state.Item1 != null && state.Item1.Success && state.Item1.Data is JObject)
                    {
                        var data = CryptsyMarketInfo.ReadMultipleFromJObject((JObject) state.Item1.Data, basicInfoOnly);
                        data.ToList().ForEach(resultList.Add);
                    }
                });
             sw.Stop();

            return resultList;
        }

        public async Task<Exchange.Cryptsy.CryptsyOrderBook> GetOrderBook(long marketId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPublicQuery(new Dictionary<string, string> { { "method", "singleorderdata" }, { "marketid", Convert.ToString(marketId) } }, false, token);

                //Response is an array of markets, with only the requested market
                return response.Success ? CryptsyOrderBook.ReadMultipleFromJObject(response.Data as JObject)[marketId] : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        //Returns: Dictionary<MarketID, OrderBook>
        public async Task<Dictionary<Int64, CryptsyOrderBook>> GetAllOrderBooks(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPublicQuery(new Dictionary<string, string> { { "method", "orderdata" } }, false, token);

                //Response is an array of markets, with only the requested market
                return response.Success ? CryptsyOrderBook.ReadMultipleFromJObject(response.Data as JObject) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }
        #endregion

        #region Private Api Methods
        public async Task<CryptsyAccountBalance> GetBalance(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(new Dictionary<string, string> { { "method", "getinfo" } }, token, RequestCategory.AccountHoldings);

                return response.Success ? CryptsyAccountBalance.ReadFromJObject(response.Data as JObject) : null;
            }, TimeSpan.FromMilliseconds(Constant.CryptsyRetryInterval));
        }

        public async Task<decimal> CalculateFee(CryptsyOrderType orderType, decimal quantity, decimal price, CancellationToken token = default(CancellationToken))
        {
            var culture = CultureHelper.GetEnglishCulture();
            return await RetryHelper.DoAsync(async () =>
            {
                if (orderType == CryptsyOrderType.Na) return -1;

                CryptsyResponse response = await CryptsyPrivateQuery(
                    new Dictionary<string, string>
                {
                    { "method", "calculatefees" }, 
                    { "ordertype", orderType == CryptsyOrderType.Buy ? "Buy" : "Sell" }, 
                    { "quantity", Convert.ToString(quantity, culture.NumberFormat) }, 
                    { "price", Convert.ToString(price, culture.NumberFormat) }
                }, token);

                return response.Success && response.Data != null ? response.Data.Value<decimal>("fee") : -1;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> GenerateNewAddress(string currencyCode, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(
                new Dictionary<string, string> { { "method", "generatenewaddress" }, { "currencycode", currencyCode } },
                token);

                return response.Success && response.Data != null ? response.Data.Value<string>("address") : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<List<CryptsyTrade>> GetMarketTrades(long marketId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(
                new Dictionary<string, string> { { "method", "markettrades" }, { "marketid", Convert.ToString(marketId) } },
                token);

                //Response is an array of markets, with only the requested market
                return response.Success ? CryptsyTrade.ReadMultipleFromJArray(response.Data as JArray) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<List<CryptsyTrade>> GetMyTrades(long marketId, uint limitResults = 200, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(
                new Dictionary<string, string> { { "method", "mytrades" }, { "marketid", Convert.ToString(marketId) }, { "limit", Convert.ToString(limitResults) } },
                token);

                //Response is an array of markets, with only the requested market
                return response.Success ? CryptsyTrade.ReadMultipleFromJArray(response.Data as JArray) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<List<CryptsyOrder>> GetMyOrders(long marketId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(
                new Dictionary<string, string> { { "method", "myorders" }, { "marketid", Convert.ToString(marketId) } },
                token, RequestCategory.OpenOrders);

                //Response is an array of markets, with only the requested market
                return response.Success ? CryptsyOrder.ReadMultipleFromJArray(response.Data as JArray, marketId) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<List<CryptsyOrder>> GetAllMyOrders(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(new Dictionary<string, string> { { "method", "allmyorders" } },
                    token, RequestCategory.OpenOrders);

                //Response is an array of markets, with only the requested market
                return response.Success ? CryptsyOrder.ReadMultipleFromJArray(response.Data as JArray) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<List<CryptsyTrade>> GetAllMyTrades(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(new Dictionary<string, string> { { "method", "allmytrades" } }, token);

                //Response is an array of markets, with only the requested market
                return response.Success ? CryptsyTrade.ReadMultipleFromJArray(response.Data as JArray) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        //Gets withdrawals & deposits
        public async Task<List<CryptsyTransaction>> GetTransactions(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(new Dictionary<string, string> { { "method", "mytransactions" } }, token);

                return response.Success ? CryptsyTransaction.ReadMultipleFromJArray(response.Data as JArray) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<CryptsyOrderResult> CreateOrder(long marketId, CryptsyOrderType orderType, decimal quantity, decimal price, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (orderType == CryptsyOrderType.Na) return new CryptsyOrderResult { Success = false, OrderId = -1, Message = "orderType must be BUY or SELL." };

                CryptsyResponse response = await CryptsyPrivateQuery(new Dictionary<string, string> { 
                { "method", "createorder" },
                {"marketid", Convert.ToString(marketId) },
                {"ordertype", orderType == CryptsyOrderType.Buy ? "Buy" : "Sell" },
                {"quantity", Convert.ToString(quantity, CultureInfo.InvariantCulture) },
                {"price", Convert.ToString(price, CultureInfo.InvariantCulture) } }, token, RequestCategory.SubmitOrder);

                return response.Success ?
                    new CryptsyOrderResult { Success = true, OrderId = response.OrderId, Message = response.Info } :
                    new CryptsyOrderResult { Success = false, OrderId = -1, Message = response.Error };
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<string> BuyLimit(int marketId, Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order.MarketSide != MarketSide.Bid)
                    throw new ApplicationException("Cryptsy API, Incorrect market side: {0}".FormatAs(order));
                var data = await CreateOrder(marketId, CryptsyOrderType.Buy, order.Amount, order.Price, token);
                if (data.Success)
                {
                    return data.OrderId.ToString(CultureInfo.InvariantCulture);
                }
                throw new ApplicationException("Cryptsy API, can't create order: {0}".FormatAs(order) +
                                               $" Error: {data.Message}");
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<string> SellLimit(int marketId, Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                if (order.MarketSide != MarketSide.Ask)
                    throw new ApplicationException("Cryptsy API, Incorrect market side: {0}".FormatAs(order));

                var data = await CreateOrder(marketId, CryptsyOrderType.Sell, order.Amount, order.Price, token);
                if (data.Success)
                {
                    return data.OrderId.ToString(CultureInfo.InvariantCulture);
                }
                throw new CryptsyException("Cryptsy API, can't create order: {0}".FormatAs(order) +
                                           $" Error: {data.Message}");
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<CryptsyOrderResult> CancelOrder(long orderId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(
                new Dictionary<string, string> 
                { 
                    { "method", "cancelorder" },
                    {"orderid", Convert.ToString(orderId) }
                }, token, RequestCategory.CancelOrder);

                return new CryptsyOrderResult { Success = response.Success, OrderId = orderId, Message = response.Info ?? Convert.ToString(response.Data) };
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<IEnumerable<AccountChange>> GetAccountHoldings(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var accountHoldings = await GetBalance(token);

                return accountHoldings?.BalanceAvailable
                    .Select(
                        ah => new AccountChange(ExchangeName.Cryptsy.ToString(), ah.Key, ah.Value)) ?? new List<AccountChange>(); 
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var orderStatus = await GetCryptsyOrderStatus(order, token);
                var avgPrice = orderStatus.TradeInfo != null && orderStatus.TradeInfo.Count > 0 ? orderStatus.TradeInfo.Average(x => x.UnitPrice) : -1;
                var totalFilledAmount = orderStatus.TradeInfo != null && orderStatus.TradeInfo.Count > 0 ? orderStatus.TradeInfo.Average(x => x.Quantity) : -1;
                return Tuple.Create(avgPrice, totalFilledAmount);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = new CancellationToken())
        {
            return RetryHelper.Do(async () =>
            {
                if (order == null)
                    throw new ArgumentNullException(nameof(order), "CryptsyApi GetOrderStatus method.");

                if (!string.IsNullOrEmpty(order.Id))
                {
                    var orderStatus = await GetCryptsyOrderStatus(order, token);
                    var avgPrice = orderStatus.TradeInfo != null && orderStatus.TradeInfo.Count > 0 ? orderStatus.TradeInfo.Average(x => x.UnitPrice) : 0m;
                    var totalFilledAmount = orderStatus.TradeInfo != null && orderStatus.TradeInfo.Count > 0 ? orderStatus.TradeInfo.Average(x => x.Quantity) : 0;

                    if (!orderStatus.Active && orderStatus.RemainQty == 0m && avgPrice != 0m && totalFilledAmount == order.Amount)
                        return new OrderChange(order, order.Amount, avgPrice, OrderStatus.Filled, DateTime.UtcNow, order.Amount);

                    if (orderStatus.Active && avgPrice != 0m && totalFilledAmount < order.Amount)
                        return new OrderChange(order, 0, avgPrice, OrderStatus.PartiallyFilled, DateTime.UtcNow, totalFilledAmount);
                }

                return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);

            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }

        public async Task<CryptsyOrderStatusInfo> GetCryptsyOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(new Dictionary<string, string> 
                { 
                    { "method", "getorderstatus" },
                    { "orderid", Convert.ToString(order.Id) }
                }, token, RequestCategory.OrderStatus);

                var orderStatus = response.Data["orderinfo"].ToObject<CryptsyOrderStatusInfo>();
                orderStatus.TradeInfo = response.Data["tradeinfo"].ToObject<List<CryptsyTrade>>();

                return orderStatus;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        //Returns null if unsuccessful, otherwise list of info-messages
        public async Task<List<string>> CancelAllMarketOrders(long marketId, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(
                    new Dictionary<string, string> 
                    { 
                        { "method", "cancelmarketorders" },
                        {"marketid", Convert.ToString(marketId) }
                    }, token);

                if (response.Success)
                {
                    var r = new List<string>();
                    if (response.Data != null) //Any orders canceled?
                    {
                        r.AddRange(response.Data.Select(Convert.ToString));
                    }
                    return r;
                }

                return null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        //Returns null if unsuccessful, otherwise list of info-messages
        public async Task<List<string>> CancelAllOrders(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                CryptsyResponse response = await CryptsyPrivateQuery(
                new Dictionary<string, string>
                {
                    { "method", "cancelallorders" }
                }, token, RequestCategory.CancelAllOrder);

                if (response.Success)
                {
                    var r = new List<string>();
                    if (response.Data != null) //Any orders canceled?
                    {
                        r.AddRange(response.Data.Select(Convert.ToString));
                    }

                    return r;
                }

                return null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }
    
        #endregion

        #region Query
        private async Task<CryptsyResponse> CryptsyPublicQuery(Dictionary<string, string> args, bool urlSwitcher = false, CancellationToken token = default(CancellationToken))
        {
            var data = await PublicQuery(args, urlSwitcher, token);
            return GetCryptsyResponse(ref data);
        }

        private async Task<string> PublicQuery(Dictionary<string, string> args, bool urlSwitcher = false, CancellationToken token = default(CancellationToken))
        {
            var dataStr = BuildPostData(args);
            string url = urlSwitcher
                ? _apiFirstDataUri.AbsoluteUri + "?" + dataStr
                : _apiSecondDataUri.AbsoluteUri + "?" + dataStr;

            return await QueryHelper.Query(url, token, false);
        }

        private async Task<CryptsyResponse> CryptsyPrivateQuery(Dictionary<string, string> args, 
            CancellationToken token = default(CancellationToken), 
            RequestCategory requestCategory = RequestCategory.Ignore)
        {
            var data = await PrivateQuery(args, token);
            //FileStorageHelper.StoreFile(ExchangeName.Cryptsy, _apiUri.AbsoluteUri, ConvertorHelper.DictionaryToJson(args), data, requestCategory, _log);
            LastMessage = data;
            return GetCryptsyResponse(ref data);
        }

        private CryptsyResponse GetCryptsyResponse(ref string data)
        {
            var jsonData = new string(data.ToCharArray());
            JObject result = JObject.Parse(data);
            data = null;
            CryptsyResponse cryptsyResponse = CryptsyResponse.ReadFromJObject(result);

            if (!string.IsNullOrEmpty(cryptsyResponse.Error))
            {
                //_log.Error($"Cryptsy Response Error : {cryptsyResponse.Error} , Data(json) : {jsonData}");
                LastMessage = cryptsyResponse.Error;
            }

            return cryptsyResponse;
        }

        private async Task<string> PrivateQuery(Dictionary<string, string> args, CancellationToken token = default(CancellationToken))
        {
            string apiUrl = _apiUri.AbsoluteUri;

            QueryHelper.SetServicePointManagerSettings(false);

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);

                args["nonce"] = GetNonce().ToString("D");
                var parameters = BuildPostData(args);

                client.DefaultRequestHeaders.Add("Key", _apiKey);
                client.DefaultRequestHeaders.Add("Sign", GetHMac(parameters, _apiSecret).ToLower());

                var content = new FormUrlEncodedContent(args);
                content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                using (var response = await client.PostAsync(apiUrl, content, token))
                {
                    return await response.Content.ReadAsStringAsync();
                }
            }
        }

        #endregion

        public CryptsyMarket[] GetMarkets(CancellationToken token = default(CancellationToken))
        {
            return RetryHelper.Do(() =>
            {
                string content = PrivateQuery(new Dictionary<string, string> { { "method", "getmarkets" } }, token).Result;
                JObject o = JObject.Parse(content);
                if (int.Parse(o["success"].ToString()) != 1) throw new SyntaxErrorException(o["error"].ToString());

                var deserialized = JsonConvert.DeserializeObject<CryptsyMarket[]>(o["return"].ToString());
                return deserialized;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #region Utils
        private static string BuildPostData(Dictionary<string, string> d)
        {
            string s = string.Empty;
            
            for (int i = 0; i < d.Count; i++)
            {
                var item = d.ElementAt(i);
                var key = item.Key;
                var val = item.Value;

                s += $"{key}={HttpUtility.UrlEncode(val)}";

                if (i != d.Count - 1)
                    s += "&";
            }

            return s;
        }

        private static string GetHMac(string message, string key)
        {
            byte[] keyByte = Encoding.UTF8.GetBytes(key);
            var hmacsha256 = new HMACSHA512(keyByte);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            hmacsha256.ComputeHash(messageBytes);

            return ByteToString(hmacsha256.Hash);
        }

        private static string ByteToString(IEnumerable<byte> buff)
        {
            return buff.Aggregate(string.Empty, (current, t) => current + t.ToString("X2"));
        }

        private static BigInteger _multiplier = 1;
        private static BigInteger _lastNonce = 0;

        private static BigInteger GetNonce()
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
        #endregion
        
    }

}

