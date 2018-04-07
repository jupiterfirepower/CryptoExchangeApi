using Newtonsoft.Json;

namespace Exchange.Bter.Responses
{
    public class BterResponse
    {
        private const string Success = "Success";

        [JsonProperty(PropertyName = "result")]
        public bool Result { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "msg")]
        public string Msg { get; set; }

        [JsonProperty(PropertyName = "order_id")]
        public string OrderId { get; set; }

        public bool IsSuccess
        {
            get { return Msg == Success; }
        }
    }
}
