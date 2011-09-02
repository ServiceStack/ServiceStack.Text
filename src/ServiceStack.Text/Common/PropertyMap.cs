using System ;
using System.Collections.Generic ;

namespace ServiceStack.Text.Common
{
	/// <summary>
	/// Represents a type that exposes parsers and <see cref="SetPropertyDelegate"/>s for a given type and property name.
	/// </summary>
	internal class PropertyMap
	{
		readonly Dictionary<string, SetPropertyDelegate> _setterMap = new Dictionary<string, SetPropertyDelegate>();
		readonly Dictionary<string, ParseStringDelegate> _map = new Dictionary<string, ParseStringDelegate>();

		public PropertyMap( Type type, ITypeSerializer serializer )
		{
			if (!type.IsClass)
			{
				throw new ArgumentException(@"Cannot construct a property map for a type that isn't a class.");
			}
				
			var propertyInfos = type.GetProperties();

			_setterMap = new Dictionary<string, SetPropertyDelegate>();
			_map = new Dictionary<string, ParseStringDelegate>();

			foreach (var propertyInfo in propertyInfos)
			{
				_map[propertyInfo.Name] = serializer.GetParseFn(propertyInfo.PropertyType);
				_setterMap[propertyInfo.Name] = ParseUtils.GetSetPropertyMethod(type, propertyInfo);
			}
		}

		public ParseStringDelegate TryGetParserFor(string name)
		{
			return _map.ContainsKey( name ) ? _map[ name ] : null ;
		}

		public SetPropertyDelegate TryGetSetterFor( string name )
		{
			SetPropertyDelegate retVal ;
			_setterMap.TryGetValue( name, out retVal ) ;
			return retVal ;
		}
	}
}