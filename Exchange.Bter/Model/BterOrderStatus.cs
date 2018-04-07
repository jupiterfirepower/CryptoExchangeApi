using System;
using Exchange.Bter.Enums;
using Newtonsoft.Json;

namespace Exchange.Bter.Model
{
    public class BterOrderStatus
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }
        [JsonProperty(PropertyName = "pair")]
        public string Pair { get; set; }
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }
        [JsonProperty(PropertyName = "rate")]
        public decimal Rate { get; set; }
        [JsonProperty(PropertyName = "Amount")]
        public decimal Amount { get; set; }
        [JsonProperty(PropertyName = "initial_rate")]
        public int InitialRate { get; set; }
        [JsonProperty(PropertyName = "initial_amount")]
        public decimal InitialAmount { get; set; }

        public BterOrderType BterOrderType
        {
            get
            {
                BterOrderType result;

                if (!String.IsNullOrEmpty(Type))
                {
                    Enum.TryParse(Type, out result);
                }
                else
                {
                    result = BterOrderType.Unknown;
                }

                return result;
            }
        }
    }
}
