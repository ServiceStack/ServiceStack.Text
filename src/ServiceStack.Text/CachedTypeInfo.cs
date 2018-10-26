using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;

namespace ServiceStack.Text
{
    public class CachedTypeInfo
    {
        static Dictionary<Type, CachedTypeInfo> CacheMap = new Dictionary<Type, CachedTypeInfo>();

        public static CachedTypeInfo Get(Type type)
        {
            if (CacheMap.TryGetValue(type, out CachedTypeInfo value))
                return value;

            var instance = new CachedTypeInfo(type);

            Dictionary<Type, CachedTypeInfo> snapshot, newCache;
            do
            {
                snapshot = CacheMap;
                newCache = new Dictionary<Type, CachedTypeInfo>(CacheMap)
                {
                    [type] = instance
                };
            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref CacheMap, newCache, snapshot), snapshot));

            return instance;
        }

        public CachedTypeInfo(Type type)
        {
            EnumInfo = EnumInfo.GetEnumInfo(type);
        }

        internal readonly EnumInfo EnumInfo;
    }
    
    public class EnumInfo
    {
        internal EnumInfo(Type enumType)
        {
            var enumValues = Enum.GetValues(enumType);
            foreach (var enumValue in enumValues)
            {
                var mi = enumType.GetMember(enumValue.ToString());
                var enumMemberAttr = mi[0].FirstAttribute<EnumMemberAttribute>();
                if (enumMemberAttr?.Value != null)
                {
                    if (enumMemberValues == null)
                        enumMemberValues = new Dictionary<object, object>();
                    enumMemberValues[enumValue] = enumMemberAttr.Value;
                }
            }
            isEnumFlag = enumType.IsEnumFlags();
        }

        private readonly bool isEnumFlag;
        private readonly Dictionary<object, object> enumMemberValues;

        public static EnumInfo GetEnumInfo(Type type)
        {
            if (type.IsEnum)
                return new EnumInfo(type);
            
            var nullableType = Nullable.GetUnderlyingType(type);           
            if (nullableType?.IsEnum == true)
                return new EnumInfo(nullableType);

            return null;
        }

        public object GetSerializedValue(object enumValue)
        {
            if (enumMemberValues != null && enumMemberValues.TryGetValue(enumValue, out var memberValue))
                return memberValue;
            if (isEnumFlag || JsConfig.TreatEnumAsInteger)
                return enumValue;
            return enumValue.ToString();
        }
    }
    
}