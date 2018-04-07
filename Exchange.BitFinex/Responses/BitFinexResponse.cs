using Newtonsoft.Json;

namespace BitFinex.Responses
{
    public class BitFinexResponse
    {
        [JsonProperty(PropertyName = "result")]
        public string Result { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        public bool IsSuccess => string.IsNullOrEmpty(Message);
    }
}
