using System;
using System.Runtime.Serialization;

namespace Exchange.Kraken.Exceptions
{
    public class KrakenInsufficientFundsException : Exception
    {
        public KrakenInsufficientFundsException()
        {
        }

        public KrakenInsufficientFundsException(string message): base(message)
        {
        }

        public KrakenInsufficientFundsException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected KrakenInsufficientFundsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
