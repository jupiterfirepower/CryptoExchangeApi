using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Bitstamp.Model;
using Exchange.Bitstamp.Helper;
using Newtonsoft.Json;

namespace Exchange.Bitstamp
{
    public partial class BitStampApi : ApiBase, IBitStampApi
    {
        #region Private Query
        
        private NameValueCollection GetPrivateDefault()
        {
            var nonce = GetNonce();
            var signature = GetSignature(nonce);

            return new NameValueCollection
                        {
                           { "key", _apiKey },
                           { "signature", signature },
                           { "nonce", nonce.ToString("D") }
                        };
        }


        private async Task<string> PrivateQuery(string url, NameValueCollection collection, 
            CancellationToken token = default(CancellationToken), 
            RequestCategory requestCategory = RequestCategory.Ignore)
        {
            QueryHelper.SetServicePointManagerSettings();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);

                var content = new FormUrlEncodedContent(collection.AsKeyValuePair());

                using (var response = await client.PostAsync(url, content, token))
                {
                    var data = await response.Content.ReadAsStringAsync();
                    //Log.Info("BitSamp nonce - {0}", collection["nonce"]);
                    LogErrors(data, collection["nonce"]);
                    //FileStorageHelper.StoreFile(ExchangeName.BitStamp, url, ConvertorHelper.DictionaryToJson(collection), data, requestCategory, Log);
                    return data;
                }
            }
        }

        #region Log Error
        private void LogErrors(string data, string nonce)
        {
            if (data.StartsWith("{\"error\""))
            {
                //Log.Error($"BitStamp Error Response: {data}, Nonce - {nonce}");
            }
        }
        #endregion

        #endregion

        #region Public Methods

        public async Task<Dictionary<string, string>> GetBalance(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal(); 

                return await RetryHelper.DoAsync(async () =>
                {
                    var data = await PrivateQuery("https://www.bitstamp.net/api/balance/", GetPrivateDefault(), token, RequestCategory.AccountHoldings);
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<BitStampUserTransaction>> GetUserTransactions(int skip = 0, int limit = 1000, string sort = "desc", CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var collection = GetPrivateDefault();
                    collection.Add("limit", Convert.ToString(limit));
                    collection.Add("skip", Convert.ToString(skip));
                    collection.Add("sort", sort);

                    var data = await PrivateQuery("https://www.bitstamp.net/api/user_transactions/", GetPrivateDefault(), token, RequestCategory.OrderHistory);
                    return JsonConvert.DeserializeObject<List<BitStampUserTransaction>>(data).Where(x => x.Type == 2); //type - transaction type (0 - deposit; 1 - withdrawal; 2 - market trade)
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<IEnumerable<BitStampOrder>> GetUserOpenOrders(CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var data = await PrivateQuery("https://www.bitstamp.net/api/open_orders/", GetPrivateDefault(), token, RequestCategory.OpenOrders);
                    return JsonConvert.DeserializeObject<List<BitStampOrder>>(data);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                AutoResetEventSet();
            }
        }

        public async Task<bool> CancelOrder(string id, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var paramCollection = GetPrivateDefault();
                    paramCollection.Add("id", id);
                    var data = await PrivateQuery("https://www.bitstamp.net/api/cancel_order/", paramCollection, token, RequestCategory.CancelOrder);

                    bool result;
                    bool.TryParse(data, out result);
                    return result;
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                WaitResourceFreeSignal();
            }
        }

        public async Task<BitStampOrder> BuyLimit(decimal amount, decimal price, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var culture = CultureHelper.GetEnglishCulture();
                return await BuyLimit(Convert.ToString(decimal.Round(amount, 8), culture.NumberFormat), Convert.ToString(Decimal.Round(price, 2), culture.NumberFormat), token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }
       
        public async Task<BitStampOrder> SellLimit(decimal amount, decimal price, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var culture = CultureHelper.GetEnglishCulture();
                return await SellLimit(Convert.ToString(decimal.Round(amount, 8), culture.NumberFormat), Convert.ToString(Decimal.Round(price, 2), culture.NumberFormat), token);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
        }
        #endregion

        #region Private Methods
        private async Task<BitStampOrder> BuyLimit(string amount, string price, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    var paramCollection = GetPrivateDefault();
                    paramCollection.Add("amount", amount);
                    paramCollection.Add("price", price);

                    var data = await PrivateQuery("https://www.bitstamp.net/api/buy/", paramCollection, token, RequestCategory.SubmitOrder);
                    return JsonConvert.DeserializeObject<BitStampOrder>(data);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                WaitResourceFreeSignal();
            }
        }

        private async Task<BitStampOrder> SellLimit(string amount, string price, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();
                return await RetryHelper.DoAsync(async () =>
                {
                    var paramCollection = GetPrivateDefault();
                    paramCollection.Add("amount", amount);
                    paramCollection.Add("price", price);

                    var data = await PrivateQuery("https://www.bitstamp.net/api/sell/", paramCollection, token, RequestCategory.SubmitOrder);
                    return JsonConvert.DeserializeObject<BitStampOrder>(data);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
            }
            finally
            {
                WaitResourceFreeSignal();
            }
        }
        #endregion

        public async Task<Tuple<decimal, decimal>> GetAvgPriceAndTotalFilledAmount(Order order, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    decimal totalFillAmount = -1;
                    decimal avgPrice = -1;

                    var userTransactions = (await GetUserTransactions(0, 1000, "desc", token)).ToArray();

                    if (userTransactions.Any(x => Convert.ToString(x.OrderId) == order.Id))
                    {
                        totalFillAmount = userTransactions.Sum(x => Math.Abs(x.Btc));
                        avgPrice = userTransactions.Average(x => x.BtcUsd);
                    }

                    return Tuple.Create(avgPrice, totalFillAmount);

                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
            }
            finally
            {
                WaitResourceFreeSignal();
            }
        }

        #region GetOrderStatus
        public async Task<OrderChange> GetOrderStatus(Order order, CancellationToken token = default(CancellationToken))
        {
            try
            {
                WaitResourceFreeSignal();

                return await RetryHelper.DoAsync(async () =>
                {
                    if (order == null)
                        throw new ArgumentNullException(nameof(order), "BitStampApi GetOrderStatus method.");

                    if (!string.IsNullOrEmpty(order.Id))
                    {
                        var userTransactions = (await GetUserTransactions(0, 1000, "desc", token)).ToArray();

                        if (userTransactions.Any(x => Convert.ToString(x.OrderId) == order.Id))
                        {
                            var amount = userTransactions.Where(x => Convert.ToString(x.OrderId) == order.Id).Sum(x => Math.Abs(x.Btc));
                            var avgPrice = userTransactions.Where(x => Convert.ToString(x.OrderId) == order.Id).Average(x => x.BtcUsd);

                            if (amount == order.Amount)
                            {
                                return new OrderChange(order, order.Amount, avgPrice, OrderStatus.Filled, DateTime.UtcNow, order.Amount);
                            }

                            if (amount < order.Amount)
                            {
                                return new OrderChange(order, 0, avgPrice, OrderStatus.PartiallyFilled, DateTime.UtcNow, amount);
                            }
                        }
                    }

                    return new OrderChange(order, 0, 0, OrderStatus.Unknown, DateTime.UtcNow);
                }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval), 1);
            }
            finally
            {
                WaitResourceFreeSignal();
            }
        }
        #endregion

        #region Utils
        private static BigInteger _multiplier = 1000;
        private static BigInteger _lastNonce = 0;
        private readonly ThreadLocal<Random> _localRandom = new ThreadLocal<Random>(() => new Random(new Random().Next()));
        private readonly object _locker = new object();

        private BigInteger GetNonce()
        {
            lock (_locker)
            {
                var currentNonce = BigInteger.Abs(new BigInteger(Math.Abs(DateTime.UtcNow.AddDays(1).Ticks) + Math.Abs(_localRandom.Value.Next())) * _multiplier);
                _lastNonce = currentNonce = currentNonce > _lastNonce ? currentNonce : currentNonce + BigInteger.Abs(_lastNonce - currentNonce) + 1;
                return currentNonce;
            }
        }

        private string GetSignature(BigInteger nonce)
        {
            string msg = $"{nonce.ToString("D")}{_apiUserName}{_apiKey}";

            return ByteArrayTostring(SignHmacsha256(
                _apiSecret, StrinToByteArray(msg))).ToUpper();
        }

        private static byte[] SignHmacsha256(string key, byte[] data)
        {
            var hashMaker = new HMACSHA256(Encoding.ASCII.GetBytes(key));
            return hashMaker.ComputeHash(data);
        }

        private static string ByteArrayTostring(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private static byte[] StrinToByteArray(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
        #endregion
    }
}
