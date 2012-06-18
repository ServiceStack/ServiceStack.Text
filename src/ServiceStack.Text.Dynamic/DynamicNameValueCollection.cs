using System.Collections.Specialized;
using System.Linq;

namespace ServiceStack.Text.Dynamic
{
    public class DynamicNameValueCollection : DynamicDictionary
    {
        public DynamicNameValueCollection(NameValueCollection nameValueCollection) : base(nameValueCollection.AllKeys.ToDictionary(x => x, x => nameValueCollection[x])) { }
    }
}