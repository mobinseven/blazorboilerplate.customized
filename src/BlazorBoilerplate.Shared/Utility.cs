using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace BlazorBoilerplate.Shared
{
    public static class Utility
    {
        public static string PersianToEnglish(this string persianStr)
        {
            Dictionary<string, string> PersianLettersDictionary = new Dictionary<string, string>
            {
                ["۰"] = "0",
                ["۱"] = "1",
                ["۲"] = "2",
                ["۳"] = "3",
                ["۴"] = "4",
                ["۵"] = "5",
                ["۶"] = "6",
                ["۷"] = "7",
                ["۸"] = "8",
                ["۹"] = "9"
            };
            return PersianLettersDictionary.Aggregate(persianStr.ArabicToEnglish(), (current, item) =>
                         current.Replace(item.Key, item.Value));
        }

        public static string ArabicToEnglish(this string persianStr)
        {
            Dictionary<string, string> ArabicLettersDictionary = new Dictionary<string, string>
            {
                ["٠"] = "0",
                ["١"] = "1",
                ["٢"] = "2",
                ["٣"] = "3",
                ["٤"] = "4",
                ["٥"] = "5",
                ["٦"] = "6",
                ["٧"] = "7",
                ["٨"] = "8",
                ["٩"] = "9"
            };
            return ArabicLettersDictionary.Aggregate(persianStr, (current, item) =>
                         current.Replace(item.Key, item.Value));
        }

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