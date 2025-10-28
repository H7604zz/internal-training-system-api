using System;

namespace InternalTrainingSystem.Core.Utils
{
    public static class DateTimeUtils
    {
        // Windows timezone id for UTC+7 (Vietnam)
        private const string VnTimeZoneId = "SE Asia Standard Time";

        private static TimeZoneInfo? _vnTimeZone;

        private static TimeZoneInfo VnTimeZone
        {
            get
            {
                if (_vnTimeZone == null)
                {
                    try
                    {
                        _vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(VnTimeZoneId);
                    }
                    catch
                    {
                        // Fallback to UTC if timezone not found (e.g., on some Linux hosts)
                        _vnTimeZone = TimeZoneInfo.Utc;
                    }
                }
                return _vnTimeZone;
            }
        }

        // Returns current Vietnam local time (UTC+7 on Windows). Falls back to UTC when timezone is unavailable.
        public static DateTime Now()
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, VnTimeZone);
        }
    }
}
