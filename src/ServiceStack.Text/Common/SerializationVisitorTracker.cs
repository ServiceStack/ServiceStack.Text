using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ServiceStack.Text.Common
{
    internal static class SerializationVisitorTracker
    {
        static Dictionary<TextWriter, List<object>> VisitedCache = new Dictionary<TextWriter, List<object>>();

        internal static void TrackSerialization(TextWriter writer)
        {
            lock (VisitedCache)
            {
                VisitedCache.Add(writer, new List<object>());
            }
        }

        internal static void UnTrackSerialization(TextWriter writer)
        {
            lock (VisitedCache)
            {
                VisitedCache.Remove(writer);
            }
        }

        internal static bool HasVisited(TextWriter writer, object o)
        {
            lock (VisitedCache)
            {
                return VisitedCache[writer].Exists(x => Object.ReferenceEquals(x, o));
            }
        }

        internal static void Track(TextWriter writer, object o)
        {
            lock (VisitedCache)
            {
                VisitedCache[writer].Add(o);
            }
        }
    }


}
