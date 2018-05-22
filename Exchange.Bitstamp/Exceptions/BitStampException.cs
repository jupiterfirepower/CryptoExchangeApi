using System;
using System.Runtime.Serialization;

namespace Exchange.Bitstamp.Exceptions
{
    [Serializable]
    public class BitStampException : Exception
    {
        public BitStampException()
        {
        }

        public BitStampException(string message)
            : base(message)
        {
        }

        public BitStampException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BitStampException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
