using System;
using System.Runtime.Serialization;

namespace Exchange.Bittrex.Exceptions
{
    [Serializable]
    public class BittrexException : Exception
    {
        public BittrexException()
        {
        }

        public BittrexException(string message): base(message)
        {
        }

        public BittrexException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected BittrexException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
