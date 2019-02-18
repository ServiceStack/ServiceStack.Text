using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceStack
{
    /// <summary>
    /// UX friendly alternative alias of Dictionary&lt;string, object&gt;
    /// </summary>
    public class ObjectDictionary : Dictionary<string, object>
    {
        public ObjectDictionary() { }
        public ObjectDictionary(int capacity) : base(capacity) { }
        public ObjectDictionary(IEqualityComparer<string> comparer) : base(comparer) { }
        public ObjectDictionary(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer) { }
        public ObjectDictionary(IDictionary<string, object> dictionary) : base(dictionary) { }
        public ObjectDictionary(IDictionary<string, object> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer) { }
        protected ObjectDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }

    public class StringDictionary : Dictionary<string, string>
    {
        public StringDictionary() { }
        public StringDictionary(int capacity) : base(capacity) { }
        public StringDictionary(IEqualityComparer<string> comparer) : base(comparer) { }
        public StringDictionary(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer) { }
        public StringDictionary(IDictionary<string, string> dictionary) : base(dictionary) { }
        public StringDictionary(IDictionary<string, string> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer) { }
        protected StringDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}