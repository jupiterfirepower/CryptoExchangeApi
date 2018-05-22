using System;
using System.Runtime.Serialization;

namespace Exchange.Kraken.Exceptions
{
    [Serializable]
    public class KrakenTemporaryLockOutException : Exception
    {
        public KrakenTemporaryLockOutException()
        {
        }

        public KrakenTemporaryLockOutException(string message): base(message)
        {
        }

        public KrakenTemporaryLockOutException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected KrakenTemporaryLockOutException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
