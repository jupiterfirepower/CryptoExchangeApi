using System;
using System.Runtime.Serialization;

namespace Exchange.Kraken.Exceptions
{
    [Serializable]
    public class KrakenRateLimitExceededException: Exception
    {
        public KrakenRateLimitExceededException()
        {
        }

        public KrakenRateLimitExceededException(string message): base(message)
        {
        }

        public KrakenRateLimitExceededException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected KrakenRateLimitExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
