using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Common.Contracts
{
    public static class QueryHelper
    {
        public static async Task<string> Query(string url, CancellationToken token = default(CancellationToken), bool special = true)
        {
            SetServicePointManagerSettings(special);

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);

                var response = await client.GetAsync(url, token);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
        }

        #region Special for ItBit
        private const string UserAgent = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
        private const string UserAgentKey = "user-agent";

        public static async Task<string> QueryWithUserAgent(string url, CancellationToken token = default(CancellationToken), bool special = true)
        {
            SetServicePointManagerSettings(special);

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.Timeout = TimeSpan.FromMilliseconds(Constant.TimeOut);
                client.DefaultRequestHeaders.Add(UserAgentKey, UserAgent);

                var response = await client.GetAsync(url, token);
                return await response.Content.ReadAsStringAsync();
            }
        }
        #endregion

        public static void SetServicePointManagerSettings(bool exp100Con = true)
        {
            ServicePointManager.UseNagleAlgorithm = true;
            ServicePointManager.DefaultConnectionLimit = Constant.DefaultConnectionLimit;
            ServicePointManager.MaxServicePoints = Constant.MaxServicePoints;
            ServicePointManager.MaxServicePointIdleTime = Constant.MaxIdleTime;
            ServicePointManager.CheckCertificateRevocationList = true;
            //ServicePointManager.DefaultConnectionLimit = ServicePointManager.DefaultPersistentConnectionLimit;

            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.Expect100Continue = exp100Con;
            if (exp100Con)
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12;
                //| SecurityProtocolType.Ssl3;
            }
        }

        public static bool ServerCertificateValidate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            switch (sslPolicyErrors)
            {
                case SslPolicyErrors.None:
                    break;
                case SslPolicyErrors.RemoteCertificateChainErrors:
                case SslPolicyErrors.RemoteCertificateNameMismatch:
                case SslPolicyErrors.RemoteCertificateNotAvailable:
                    {
                        var application = sender as HttpApplication;

                        if (application != null && null == application.Context)
                        {
                            Trace.WriteLine("SSL validation error: " + sslPolicyErrors);
                        }
                        else if (application != null)
                        {
                            var context = new HttpContextWrapper(application.Context);
                            Trace.WriteLine(context, "SSL validation error: " + sslPolicyErrors);
                        }
                    }
                    break;
                default:
                    Trace.WriteLine("Default SSL validation error: " + sslPolicyErrors);
                    break;
            }
            return true;
        }
    }
}
