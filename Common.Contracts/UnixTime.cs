using System;

namespace Common.Contracts
{
    public static class UnixTime
    {
        public static DateTime Epoch;
        static UnixTime()
        {
            Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        }

        public static UInt32 Now { get { return GetFromDateTime(DateTime.UtcNow); } }

        public static UInt32 GetNowAddDays(int days) { return GetFromDateTime(DateTime.UtcNow.AddDays(days)); }

        public static UInt32 GetFromDateTime(DateTime d) { return (UInt32)(d - Epoch).TotalSeconds; }
        public static DateTime ConvertToDateTime(UInt32 unixtime) { return Epoch.AddSeconds(unixtime); }
    }
}
