using System;
using System.Runtime.Serialization;

namespace BitFinex.Exceptions
{
    [Serializable]
    public class BitFinexException : Exception
    {
        public BitFinexException()
        {
        }

        public BitFinexException(string message): base(message)
        {
        }

        public BitFinexException(string message, Exception innerException): base(message, innerException)
        {
        }

        protected BitFinexException(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }
    }
}
