using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Exchange.Bittrex.Model
{
    public class BittrexBaseResult
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        [JsonProperty("message")]
        public string Message { get; set; }
        [JsonProperty("result")]
        public BittrexResult Result { get; set; }
    }
}
