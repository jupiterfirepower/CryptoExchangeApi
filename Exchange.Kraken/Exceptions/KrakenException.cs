using System;
using System.Runtime.Serialization;

namespace Exchange.Kraken.Exceptions
{
    [Serializable]
    public class KrakenException : Exception
    {
        public KrakenException()
        {
        }

        public KrakenException(string message): base(message)
        {
        }

        public KrakenException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected KrakenException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
