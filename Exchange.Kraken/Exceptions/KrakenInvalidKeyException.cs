using System;
using System.Runtime.Serialization;

namespace Exchange.Kraken.Exceptions
{
    [Serializable]
    public class KrakenInvalidKeyException: Exception
    {
        public KrakenInvalidKeyException()
        {
        }

        public KrakenInvalidKeyException(string message): base(message)
        {
        }

        public KrakenInvalidKeyException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected KrakenInvalidKeyException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
