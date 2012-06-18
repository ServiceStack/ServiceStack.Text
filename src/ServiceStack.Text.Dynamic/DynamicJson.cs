using System.Dynamic;
using System.Linq;

namespace ServiceStack.Text.Dynamic
{
    public class DynamicJson : DynamicObject
    {
        readonly JsonObject jsonObject;

        public DynamicJson(string json) : this(JsonObject.Parse(json)) { }

        DynamicJson(string json, string name)
        {
            jsonObject = JsonObject.Parse(json).All(x => x.Value == null) ? new JsonObject { { name, json } } : JsonObject.Parse(json);
        }

        DynamicJson(JsonObject jsonObject)
        {
            this.jsonObject = jsonObject;
        }
        
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (jsonObject.All(x => x.Key != binder.Name))
                return false;

            var value = jsonObject.Get(binder.Name);
            var jsonArrayObjects = JsonArrayObjects.Parse(value);
            if (jsonArrayObjects.Count == 1)
                result = new DynamicJson(value, binder.Name);
            else
                result = jsonArrayObjects.Select(x => new DynamicJson(x)).ToArray();
            return true;
        }

        public override bool TryConvert(ConvertBinder binder, out object result)
        {
            result = TypeSerializer.DeserializeFromString(jsonObject.First().Value, binder.ReturnType);
            return true;
        }

        public override string ToString()
        {
            return jsonObject.First().Value;
        }
    }
}