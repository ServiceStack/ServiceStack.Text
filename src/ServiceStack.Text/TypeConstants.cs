using System;
using System.Collections.Generic;
using System.Reflection;

namespace ServiceStack.Text
{
    public static class TypeConstants
    {
        public static readonly string[] EmptyStringArray = new string[0];
        public static readonly long[] EmptyLongArray = new long[0];
        public static readonly int[] EmptyIntArray = new int[0];
        public static readonly bool[] EmptyBoolArray = new bool[0];
        public static readonly byte[] EmptyByteArray = new byte[0];
        public static readonly object[] EmptyObjectArray = new object[0];
        public static readonly Type[] EmptyTypeArray = new Type[0];
        public static readonly FieldInfo[] EmptyFieldInfoArray = new FieldInfo[0];
        public static readonly PropertyInfo[] EmptyPropertyInfoArray = new PropertyInfo[0];

        public static readonly byte[][] EmptyByteArrayArray = new byte[0][];

        public static readonly List<string> EmptyStringList = new List<string>(0);
        public static readonly List<long> EmptyLongList = new List<long>(0);
        public static readonly List<int> EmptyIntList = new List<int>(0);
        public static readonly List<bool> EmptyBoolList = new List<bool>(0);
        public static readonly List<byte> EmptyByteList = new List<byte>(0);
        public static readonly List<object> EmptyObjectList = new List<object>(0);
        public static readonly List<Type> EmptyTypeList = new List<Type>(0);
        public static readonly List<FieldInfo> EmptyFieldInfoList = new List<FieldInfo>(0);
        public static readonly List<PropertyInfo> EmptyPropertyInfoList = new List<PropertyInfo>(0);
    }

    public static class TypeConstants<T>
    {
        public static readonly T[] EmptyArray = new T[0];
        public static readonly List<T> EmptyList = new List<T>(0);
        public static readonly HashSet<T> EmptyHashSet = new HashSet<T>();
    }
}