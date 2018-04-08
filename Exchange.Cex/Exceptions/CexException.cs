using System;
using System.Runtime.Serialization;

namespace Exchange.Cex.Exceptions
{
    [Serializable]
    public class CexException : Exception
    {
        public CexException()
        {
        }

        public CexException(string message)
            : base(message)
        {
        }

        public CexException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CexException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
