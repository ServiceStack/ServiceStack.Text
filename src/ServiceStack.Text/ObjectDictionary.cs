using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ServiceStack.Text
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
}