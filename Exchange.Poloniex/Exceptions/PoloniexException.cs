using System;
using System.Runtime.Serialization;

namespace Exchange.Poloniex.Exceptions
{
    [Serializable]
    public class PoloniexException : Exception
    {
        public PoloniexException()
        {
        }

        public PoloniexException(string message) : base(message)
        {
        }

        public PoloniexException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PoloniexException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
