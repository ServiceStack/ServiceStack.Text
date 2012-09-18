using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace ServiceStack.Text.Common
{
	internal static class DeserializeTypeRef
	{
		internal static SerializationException CreateSerializationError(Type type, string strType)
		{
			return new SerializationException(String.Format(
			"Type definitions should start with a '{0}', expecting serialized type '{1}', got string starting with: {2}",
			JsWriter.MapStartChar, type.Name, strType.Substring(0, strType.Length < 50 ? strType.Length : 50)));
		}

	    internal static SerializationException GetSerializationException(string propertyName, string propertyValueString, Type propertyType, Exception e)
	    {
	        var serializationException = new SerializationException(String.Format("Failed to set property '{0}' with '{1}'", propertyName, propertyValueString), e);
	        if (propertyName != null) {
	            serializationException.Data.Add("propertyName", propertyName);
	        }
	        if (propertyValueString != null) {
	            serializationException.Data.Add("propertyValueString", propertyValueString);
	        }
	        if (propertyType != null) {
	            serializationException.Data.Add("propertyType", propertyType);
	        }
	        return serializationException;
	    }

        internal static Dictionary<string, TypeAccessor> GetTypeAccessorMap(TypeConfig typeConfig, ITypeSerializer serializer)
        {
            var type = typeConfig.Type;

			var propertyInfos = type.GetSerializableProperties();
            if (propertyInfos.Length == 0) return null;

            var map = new Dictionary<string, TypeAccessor>(StringComparer.OrdinalIgnoreCase);

            var isDataContract = type.GetCustomAttributes(typeof(DataContractAttribute), false).Any();

            foreach (var propertyInfo in propertyInfos)
            {
                var propertyName = propertyInfo.Name;
                if (isDataContract)
                {
                    var dcsDataMember = propertyInfo.GetCustomAttributes(typeof(DataMemberAttribute), false).FirstOrDefault() as DataMemberAttribute;
                    if (dcsDataMember != null && dcsDataMember.Name != null)
                    {
                        propertyName = dcsDataMember.Name;
                    }
                }
                map[propertyName] = TypeAccessor.Create(serializer, typeConfig, propertyInfo);
            }
            return map;
        }
	}

}