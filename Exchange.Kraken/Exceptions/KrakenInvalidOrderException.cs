using System;
using System.Runtime.Serialization;

namespace Exchange.Kraken.Exceptions
{
    public class KrakenInvalidOrderException : Exception
    {
        public KrakenInvalidOrderException()
        {
        }

        public KrakenInvalidOrderException(string message)
            : base(message)
        {
        }

        public KrakenInvalidOrderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected KrakenInvalidOrderException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
