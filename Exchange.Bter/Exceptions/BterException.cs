using System;
using System.Runtime.Serialization;

namespace Exchange.Bter.Exceptions
{
    [Serializable]
    public class BterException : Exception
    {
        public BterException()
        {
        }

        public BterException(string message)
            : base(message)
        {
        }

        public BterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected BterException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
