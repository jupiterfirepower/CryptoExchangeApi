using System;
using Common.Contracts;
using Newtonsoft.Json.Linq;


namespace Exchange.Kraken.Model
{
    public class KrakenTime
    {
        public DateTime Time { get; set; }
        public string Rfc1123 { get; set; }

        public static KrakenTime ReadFromJObject(JObject o)
        {
            if (o == null)
                return null;            

            return new KrakenTime
            {
                Time = UnixTime.ConvertToDateTime(o.Value<UInt32>("unixtime")),
                Rfc1123 = o.Value<string>("rfc1123")                
            };
        }
    }
}
