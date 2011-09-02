using System ;
using System.Collections.Generic ;

namespace ServiceStack.Text.Common
{
	/// <summary>
	/// A type that acts as a lookup between a <see cref="Type"/> and a <see cref="PropertyMap"/>
	/// </summary>
	/// <typeparam name="TSerializer"></typeparam>
	public static class PropertyMapLookup< TSerializer >
	{
		static readonly Dictionary<Type, PropertyMap> _lookup = new Dictionary<Type, PropertyMap>( ) ;
		static readonly object _lock = new object( ) ;

		internal static PropertyMap PropertyMapFor( Type type, ITypeSerializer serializer )
		{
			if( !_lookup.ContainsKey( type ) )
			{
				lock( _lock )
				{
					if( !_lookup.ContainsKey( type ) )
					{
						_lookup.Add( type, new PropertyMap( type, serializer ) ) ;
					}
				}
			}

			return _lookup[ type ] ;
		}
	}
}