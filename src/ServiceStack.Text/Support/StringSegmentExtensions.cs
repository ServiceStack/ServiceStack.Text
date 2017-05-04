using System;
using System.Runtime.CompilerServices;
#if NETSTANDARD1_1
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Support
{
    public static class StringSegmentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty(this StringSegment value)
        {
            return value.Buffer == null || value.Length == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Char GetChar(this StringSegment value, int index)
        {
            return value.Buffer[value.Offset + index];
        }

        public static int IndexOfAny(this StringSegment value, char[] chars, int start, int count)
        {
            if (start < 0 || value.Offset + start > value.Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(start));
            }

            if (count < 0 || value.Offset + start + count > value.Buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            var index = value.Buffer.IndexOfAny(chars, start + value.Offset, count);
            if (index != -1)
            {
                return index - value.Offset;
            }
            else
            {
                return index;
            }
        }

        public static int IndexOfAny(this StringSegment value, char[] chars, int start)
        {
            return value.IndexOfAny(chars, start, value.Length);
        }

    }
}
