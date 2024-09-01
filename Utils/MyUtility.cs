using System.Security.Cryptography;
using System.Text;
using TimeZoneConverter;

namespace MockTestApi.Utils
{
    public class MyUtility
    {
        public static string GetRandomString(int length)
        {
            Random rnd = new Random();
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }

        public static DateTime ConvertToLocalTime(DateTime utcNow, string ianaTimeZoneId)
        {
            // Convert IANA timezone to Windows timezone ID
            var windowsTimeZoneId = TZConvert.IanaToWindows(ianaTimeZoneId);

            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById(windowsTimeZoneId);

            // Convert UTC to the specified local time zone
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, timeZone);
        }

        public static string GetSalt() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        public static string GetHash(string text)
        {
            using var sha256 = SHA256.Create();
            return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(text)));
        }
    }
}
