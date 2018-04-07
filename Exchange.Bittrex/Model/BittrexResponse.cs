using System.Collections.Generic;

namespace Exchange.Bittrex.Model
{
    internal class BittrexSingleResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public T result { get; set; }
    }
    internal class BittrexArrayResponse<T>
    {
        public bool success { get; set; }
        public string message { get; set; }
        public List<T> result { get; set; }
    }

}
