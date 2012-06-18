using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace ServiceStack.Text.Dynamic
{
    public class DynamicDictionary : DynamicObject
    {
        private readonly IDictionary<string, string> dictionary;

        public DynamicDictionary(IDictionary<string, string> dictionary)
        {
            this.dictionary = dictionary;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (dictionary.Keys.All(x => x != binder.Name))
                return false;

            result = new DynamicDictionary(new Dictionary<string, string> { { binder.Name, dictionary[binder.Name] } });
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = TypeSerializer.DeserializeFromString(dictionary.Values.First(), binder.ReturnType);
            return true;
        }

        public override string ToString()
        {
            return dictionary.Values.First();
        }
    }
}