using System;

#if NETSTANDARD1_1
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
            return value.Length;

            //this implementation of case insensitive hash code is temporary commented
            //because we need to implement fast char.ToUpperInvariant() at first
/*            int c;
            int last = value.Offset + value.Length;
            int i = value.Offset;
            int hash1 = 5381;
            int hash2 = hash1;

            while (i < last)
            {
                c = char.ToUpperInvariant(value.Buffer[i]);
                hash1 = ((hash1 << 5) + hash1) ^ c;
                if ((i += 5) >= last)
                    break;
                c = char.ToUpperInvariant(value.Buffer[i]);
                hash2 = ((hash2 << 5) + hash2) ^ c;
                i += 5;
            }

            return hash1 + (hash2 * 1566083941);
*/
        }
    }
}