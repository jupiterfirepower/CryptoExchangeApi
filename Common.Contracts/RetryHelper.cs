using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Contracts
{
    public static class RetryHelper
    {
        public static void Do(Action action, TimeSpan retryInterval, int retryCount = 3)
        {
            Do<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount);
        }
        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }

        public static async Task<T> DoAsync<T>(Func<Task<T>> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return await action();
                }
                catch (TaskCanceledException)
                {
                    throw;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }

        public async static Task<T> WebDoAsync<T>(Func<Task<T>> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return await action();
                }
                catch (WebException ex)
                {
                    /*switch (ex.Status)
                    {

                        case WebExceptionStatus.ConnectFailure:
                        case WebExceptionStatus.Timeout:
                        case WebExceptionStatus.NameResolutionFailure:
                        case WebExceptionStatus.ProtocolError:
                        case WebExceptionStatus.UnknownError:
                        default:
                            throw;
                    }*/
                    if (ex.Message.Contains("Could not create SSL/TLS secure channel"))
                    {
                        exceptions.Add(ex);
                        Thread.Sleep(retryInterval);
                    }
                }
            }

            throw new AggregateException(exceptions);
        }

        public static T WebDo<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    return action();
                }
                catch (WebException ex)
                {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
