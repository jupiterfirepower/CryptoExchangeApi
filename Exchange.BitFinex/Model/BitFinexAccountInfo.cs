using System.Collections.Generic;
using Newtonsoft.Json;

namespace BitFinex.Model
{
    public class BitFinexAccountInfo
    {
        [JsonProperty("fees")]
        public List<BitFinexFee> Fees { get; set; }
    }
}
