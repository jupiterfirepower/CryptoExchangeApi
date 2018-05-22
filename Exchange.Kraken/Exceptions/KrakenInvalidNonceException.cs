using System;
using System.Runtime.Serialization;

namespace Exchange.Kraken.Exceptions
{
    [Serializable]
    public class KrakenInvalidNonceException : Exception
    {
        public KrakenInvalidNonceException()
        {
        }

        public KrakenInvalidNonceException(string message): base(message)
        {
        }

        public KrakenInvalidNonceException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected KrakenInvalidNonceException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
