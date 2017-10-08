using System;

#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#endif

namespace ServiceStack.Text.Support
{
    public class HashedStringSegment
    {
        public StringSegment Value { get; }
        private readonly int hash;

        public HashedStringSegment(StringSegment value)
        {
            Value = value;
            hash = ComputeHashCode(value);
        }

        public HashedStringSegment(string value) : this(new StringSegment(value))
        {
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(((HashedStringSegment)obj).Value, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode() => hash;

        public static int ComputeHashCode(StringSegment value)
        {
            var length = value.Length;
            if (length == 0)
                return 0;

            var offset = value.Offset;
            var hash = 37 * length;

            char c1 = Char.ToUpperInvariant(value.Buffer[offset]);
            hash += 53 * c1;

            if (length > 1)
            {
                char c2 = Char.ToUpperInvariant(value.Buffer[offset + length - 1]);
                hash += 37 * c2;
            }

            return hash;
        }
    }
}