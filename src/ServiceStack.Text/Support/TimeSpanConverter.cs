using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ServiceStack.Text.Support
{
    public class TimeSpanConverter
    {
        public static string ToXsdDuration(TimeSpan timeSpan)
        {
            var sb = new StringBuilder("P");

            double d = timeSpan.TotalSeconds;

            int totalSeconds = (int)(d);
            int remainingMs = (int)(Math.Round(d - totalSeconds, 3) * 1000);
            int sec = (totalSeconds >= 60 ? totalSeconds % 60 : totalSeconds);
            int min = (totalSeconds = (totalSeconds / 60)) >= 60 ? totalSeconds % 60 : totalSeconds;
            int hours = (totalSeconds = (totalSeconds / 60)) >= 24 ? totalSeconds % 24 : totalSeconds;
            int days = (totalSeconds = (totalSeconds / 24)) >= 30 ? totalSeconds % 30 : totalSeconds;

            if (days > 0)
            {
                sb.Append(days + "D");
            }

            if (hours + min + sec + remainingMs > 0)
            {
                sb.Append("T");
                if (hours > 0)
                {
                    sb.Append(hours + "H");
                }
                if (min > 0)
                {
                    sb.Append(min + "M");
                }

                
                if (remainingMs > 0)
                {
                    sb.Append(sec + "." + remainingMs.ToString(CultureInfo.InvariantCulture).PadLeft(3, '0') + "S");
                }
                else if (sec > 0)
                {
                    sb.Append(sec + "S");
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
            int seconds = 0;
            double ms = 0.0;

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
                        ms = millis;
                }

                seconds = (int)ms;
                ms -= seconds;
            }

            double totalSecs = 0
                    + (days * 24 * 60 * 60)
                    + (hours * 60 * 60)
                    + (minutes * 60)
                    + (seconds);

            double interval = totalSecs + ms;

            return TimeSpan.FromSeconds(interval);
        }
    }
}
