using System;
using Newtonsoft.Json;

namespace Exchange.Cryptsy.Model
{
    public struct CryptsyMarket
    {
        [JsonProperty("Created")]
        public DateTime Created;
        [JsonProperty("Current_Volume")]
        public double CurrentVolume;
        [JsonProperty("High_Trade")]
        public double HighTrade;
        [JsonProperty("Label")]
        public string Label;
        [JsonProperty("Last_Trade")]
        public double LastTrade;
        [JsonProperty("Low_Trade")]
        public double LowTrade;
        [JsonProperty("MarketId")]
        public int MarketId;
        [JsonProperty("Primary_Currency_Code")]
        public string PrimaryCurrencyCode;
        [JsonProperty("Primary_Currency_Name")]
        public string PrimaryCurrencyName;
        [JsonProperty("Secondary_Currency_Code")]
        public string SecondaryCurrencyCode;
        [JsonProperty("Secondary_Currency_Name")]
        public string SecondaryCurrencyName;
    }
}
