//
// https://github.com/ServiceStack/ServiceStack.Text
// ServiceStack.Text: .NET C# POCO JSON, JSV and CSV Text Serializers.
//
// Authors:
//   Demis Bellot (demis.bellot@gmail.com)
//
// Copyright 2012 ServiceStack, Inc. All Rights Reserved.
//
// Licensed under the same terms of ServiceStack.
//

using System;
using System.Reflection;
#if NETSTANDARD2_0
using Microsoft.Extensions.Primitives;
#else
using ServiceStack.Text.Support;
#endif

namespace ServiceStack.Text.Common
{
    public class DeserializeTypeUtils
    {
        public static ParseStringDelegate GetParseMethod(Type type) => v => GetParseStringSegmentMethod(type)(new StringSegment(v));

        public static ParseStringSegmentDelegate GetParseStringSegmentMethod(Type type)
        {
            var typeConstructor = GetTypeStringConstructor(type);
            if (typeConstructor != null)
            {
                return value => typeConstructor.Invoke(new object[] { value.Value });
            }

            return null;
        }

        /// <summary>
        /// Get the type(string) constructor if exists
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static ConstructorInfo GetTypeStringConstructor(Type type)
        {
            foreach (var ci in type.GetConstructors())
            {
                var paramInfos = ci.GetParameters();
                var matchFound = paramInfos.Length == 1 && paramInfos[0].ParameterType == typeof(string);
                if (matchFound)
                {
                    return ci;
                }
            }
            return null;
        }

    }
}