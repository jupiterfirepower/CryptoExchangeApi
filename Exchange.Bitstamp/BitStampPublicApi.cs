using System;
using System.Collections.Generic;
using System.Configuration;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Common.Contracts;
using Exchange.Bitstamp.Helper;
using Exchange.Bitstamp.Model;
using Newtonsoft.Json;

namespace Exchange.Bitstamp
{
    public partial class BitStampApi 
    {
        private readonly Uri _baseUri = new Uri("https://www.bitstamp.net/api/");

        private string _apiKey;
        private string _apiSecret;
        private string _apiUserName;

        public BitStampApi(string apiKey, string apiSecret, string apiUserName)
            
        {
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _apiUserName = apiUserName;

            if (!String.IsNullOrEmpty(ConfigurationManager.AppSettings["BitStampMultiplier"]))
            {
                BigInteger.TryParse(ConfigurationManager.AppSettings["BitStampMultiplier"], out _multiplier);
            }
            _multiplier = _multiplier < 100000 ? 100000 : _multiplier;
        }

        #region Public Methods

        /// <summary>
        /// Ticker
        /// </summary>
        /// <returns>Tick</returns>
        public async Task<BitStampTick> GetTick(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query(_baseUri.AbsoluteUri + "ticker/", token);
                return JsonConvert.DeserializeObject<BitStampTick>(result);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        /// <summary>
        /// OrderBook
        /// </summary>
        /// <returns>OrderBook</returns>
        public async Task<BitStampOrderBook> GetOrderBook(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var url = _baseUri.AbsoluteUri + "order_book/"; //?group=1
                var result = await Query(url, token);

                return result != null ? JsonConvert.DeserializeObject<BitStampOrderBook>(result) : null;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        /// <summary>
        /// Transactions
        /// </summary>
        /// <returns>Transactions</returns>
        public async Task<List<BitStampTransact>> GetTransactions(int delta = 3600, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query(_baseUri.AbsoluteUri + "transactions/?timedelta=" + delta, token);
                return JsonConvert.DeserializeObject<List<BitStampTransact>>(result);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        /// <summary>
        /// Reserve
        /// </summary>
        /// <returns>Reserve</returns>
        public async Task<BitStampReserve> GetReserve(CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query(_baseUri.AbsoluteUri + "bitinstant/", token);
                return JsonConvert.DeserializeObject<BitStampReserve>(result);
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        public async Task<BitStampExchangeRate> GetExchangeRate(Pair pair, CancellationToken token = default(CancellationToken))
        {
            return await RetryHelper.DoAsync(async () =>
            {
                var result = await Query(_baseUri.AbsoluteUri + BitStampPairHelper.ToString(pair) + "/", token);
                var data = JsonConvert.DeserializeObject<BitStampExchangeRate>(result);
                data.Pair = pair;
                return data;
            }, TimeSpan.FromMilliseconds(Constant.DefaultRetryInterval));
        }

        #endregion

        private async Task<string> Query(string url, CancellationToken token = default(CancellationToken))
        {
            return await QueryHelper.Query(url, token);
            
        }

    }
}
