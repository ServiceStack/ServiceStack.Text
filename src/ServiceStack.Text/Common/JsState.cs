using System;
using System.Collections.Generic;

namespace ServiceStack.Text.Common
{
    internal static class JsState
    {
        //Exposing field for perf
        [ThreadStatic]
        internal static int WritingKeyCount = 0;

        [ThreadStatic]
        internal static bool IsWritingValue = false;

        [ThreadStatic]
        internal static bool IsWritingDynamic = false;

        [ThreadStatic]
        internal static bool QueryStringMode = false;

        [ThreadStatic]
        internal static HashSet<Type> InSerializerFns = new HashSet<Type>();

        internal static bool InSerialize<T>()
        {
            return InSerializerFns.Contains(typeof(T));
        }

        [ThreadStatic]
        internal static HashSet<Type> InDeserializerFns = new HashSet<Type>();

        internal static bool InDeserialize<T>()
        {
            return InDeserializerFns.Contains(typeof (T));
        }

        [ThreadStatic]
        internal static int Depth = 0;
    }
}