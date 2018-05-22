using System;
using System.Runtime.Serialization;

namespace Exchange.Cryptsy.Exceptions
{
    [Serializable]
    public class CryptsyException : Exception
    {
        public CryptsyException()
        {
        }

        public CryptsyException(string message): base(message)
        {
        }

        public CryptsyException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected CryptsyException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
