using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace ServiceStack.Text.Dynamic
{
    public class DynamicNameValueCollection : DynamicDictionary
    {
        public DynamicNameValueCollection(NameValueCollection nameValueCollection) : base(nameValueCollection.AllKeys.Aggregate(new Dictionary<string, string>(), (d, s) => { d.Add(s, nameValueCollection[s]); return d; })) { }
    }
}