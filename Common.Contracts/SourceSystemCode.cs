using System;
using System.Collections.Generic;

namespace Common.Contracts
{
    public class SourceSystemCode
    {
        public const String Unknown = "Unknown";
        public const String ExternalExchange = "ExternalExchange";
        
        public static List<string> ToList()
        {
            return new List<string>
            {
                Unknown,
                ExternalExchange
            };
        }
    }
}
