using System;
using System.Globalization;
using System.Text;

namespace BlazorBoilerplate.Shared
{
    public static class Utility
    {
        private static readonly PersianCalendar pc = new PersianCalendar();

        public static string ConvertToFormattedPersianCalendar(DateTime dt)
        {
            StringBuilder sb = new StringBuilder();
            return sb.AppendFormat("{0}:{1} | {2}/{3}/{4}",
                pc.GetHour(dt), pc.GetMinute(dt), pc.GetDayOfMonth(dt), pc.GetMonth(dt), pc.GetYear(dt)).ToString();
        }

        public static DateTime ConvertToPersian(DateTime dt)
        {
            return new DateTime(pc.GetYear(dt), pc.GetMonth(dt), pc.GetDayOfMonth(dt), pc.GetHour(dt), pc.GetMinute(dt), 0);
        }

        public static readonly DateTime UnixEpoch =
    new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long UnixTimestampMilliseconds(DateTime dt)
        {
            return (long)(dt - UnixEpoch).TotalMilliseconds;
        }

        public static long UnixTimestampSeconds(DateTime dt)
        {
            return (long)(dt - UnixEpoch).TotalSeconds;
        }
    }
}