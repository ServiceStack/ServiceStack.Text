using System;
using System.Globalization;
using System.Text;

namespace ServiceStack.Text.Support
{
    public class TimeSpanConverter
    {
        public static string ToXsdDuration(TimeSpan timeSpan)
        {
            var sb = new StringBuilder();

            sb.Append(timeSpan.Ticks < 0 ? "-P" : "P");

            double ticks = Math.Abs(timeSpan.Ticks);
            double totalSeconds = ticks / TimeSpan.TicksPerSecond;
            int wholeSeconds = (int) totalSeconds;
            int seconds = wholeSeconds;
            int sec = (seconds >= 60 ? seconds % 60 : seconds);
            int min = (seconds = (seconds / 60)) >= 60 ? seconds % 60 : seconds;
            int hours = (seconds = (seconds / 60)) >= 24 ? seconds % 24 : seconds;
            int days = seconds / 24;
            double remainingSecs = sec + (totalSeconds - wholeSeconds);

            if (days > 0)
                sb.Append(days + "D");

            if (days == 0 || hours + min + sec + remainingSecs > 0)
            {
                sb.Append("T");
                if (hours > 0)
                    sb.Append(hours + "H");

                if (min > 0)
                    sb.Append(min + "M");

                if (remainingSecs > 0)
                {
                    var secFmt = string.Format("{0:0.0000000}", remainingSecs);
                    secFmt = secFmt.TrimEnd('0').TrimEnd('.');
                    sb.Append(secFmt + "S");
                }
                else if (sb.Length == 2) //PT
                {
                    sb.Append("0S");
                }
            }

            var xsdDuration = sb.ToString();
            return xsdDuration;
        }

        public static TimeSpan FromXsdDuration(string xsdDuration)
        {
            int days = 0;
            int hours = 0;
            int minutes = 0;
            double seconds = 0;
            int sign = 1;

            if (xsdDuration.StartsWith("-", StringComparison.Ordinal))
            {
                sign = -1;
                xsdDuration = xsdDuration.Substring(1); //strip sign
            }

            string[] t = xsdDuration.Substring(1).SplitOnFirst('T'); //strip P

            var hasTime = t.Length == 2;

            string[] d = t[0].SplitOnFirst('D');
            if (d.Length == 2)
            {
                int day;
                if (int.TryParse(d[0], out day))
                    days = day;
            }
            if (hasTime)
            {
                string[] h = t[1].SplitOnFirst('H');
                if (h.Length == 2)
                {
                    int hour;
                    if (int.TryParse(h[0], out hour))
                        hours = hour;
                }

                string[] m = h[h.Length - 1].SplitOnFirst('M');
                if (m.Length == 2)
                {
                    int min;
                    if (int.TryParse(m[0], out min))
                        minutes = min;
                }

                string[] s = m[m.Length - 1].SplitOnFirst('S');
                if (s.Length == 2)
                {
                    double millis;
                    if (double.TryParse(s[0], out millis))
                        seconds = millis;
                }
            }

            double totalSecs = 0
                    + (days * 24 * 60 * 60)
                    + (hours * 60 * 60)
                    + (minutes * 60)
                    + (seconds);

            var interval = (long) (totalSecs * TimeSpan.TicksPerSecond * sign);

            return TimeSpan.FromTicks(interval);
        }
    }
}
