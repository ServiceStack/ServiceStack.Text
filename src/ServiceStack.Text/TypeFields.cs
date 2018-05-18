using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ServiceStack.Text;

using System.Linq.Expressions;

#if NET45 || NETSTANDARD2_0
using System.Reflection.Emit;
#endif

namespace ServiceStack
{
    public class FieldAccessor
    {
        public FieldAccessor(
            FieldInfo fieldInfo,
            GetMemberDelegate publicGetter,
            SetMemberDelegate publicSetter,
            SetMemberRefDelegate publicSetterRef)
        {
            FieldInfo = fieldInfo;
            PublicGetter = publicGetter;
            PublicSetter = publicSetter;
            PublicSetterRef = publicSetterRef;
        }

        public FieldInfo FieldInfo { get; }

        public GetMemberDelegate PublicGetter { get; }

        public SetMemberDelegate PublicSetter { get; }

        public SetMemberRefDelegate PublicSetterRef { get; }
    }

    public class TypeFields<T> : TypeFields
    {
        public static readonly TypeFields<T> Instance = new TypeFields<T>();

        static TypeFields()
        {
            Instance.Type = typeof(T);
            Instance.PublicFieldInfos = typeof(T).GetPublicFields();
            foreach (var fi in Instance.PublicFieldInfos)
            {
                try
                {
                    var fnRef = fi.SetExpressionRef<T>();
                    Instance.FieldsMap[fi.Name] = new FieldAccessor(
                        fi,
                        PclExport.Instance.CreateGetter(fi),
                        PclExport.Instance.CreateSetter(fi),
                        delegate (ref object instance, object arg)
                        {
                            var valueInstance = (T)instance;
                            fnRef(ref valueInstance, arg);
                            instance = valueInstance;
                        });
                }
                catch (Exception ex)
                {
                    Tracer.Instance.WriteError(ex);
                }
            }
        }

        public new static FieldAccessor GetAccessor(string propertyName)
        {
            return Instance.FieldsMap.TryGetValue(propertyName, out FieldAccessor info)
                ? info
                : null;
        }
    }

    public abstract class TypeFields
    {
        static Dictionary<Type, TypeFields> CacheMap = new Dictionary<Type, TypeFields>();

        public static Type FactoryType = typeof(TypeFields<>);

        public static TypeFields Get(Type type)
        {
            if (CacheMap.TryGetValue(type, out TypeFields value))
                return value;

            var genericType = FactoryType.MakeGenericType(type);
            var instanceFi = genericType.GetPublicStaticField("Instance");
            var instance = (TypeFields)instanceFi.GetValue(null);

            Dictionary<Type, TypeFields> snapshot, newCache;
            do
            {
                snapshot = CacheMap;
                newCache = new Dictionary<Type, TypeFields>(CacheMap)
                {
                    [type] = instance
                };
            } while (!ReferenceEquals(
                Interlocked.CompareExchange(ref CacheMap, newCache, snapshot), snapshot));

            return instance;
        }

        public FieldAccessor GetAccessor(string propertyName)
        {
            return FieldsMap.TryGetValue(propertyName, out FieldAccessor info)
                ? info
                : null;
        }

        public Type Type { get; protected set; }

        public readonly Dictionary<string, FieldAccessor> FieldsMap =
            new Dictionary<string, FieldAccessor>(PclExport.Instance.InvariantComparerIgnoreCase);

        public FieldInfo[] PublicFieldInfos { get; protected set; }

        public virtual FieldInfo GetPublicField(string name)
        {
            foreach (var fi in PublicFieldInfos)
            {
                if (fi.Name == name)
                    return fi;
            }
            return null;
        }

        public virtual GetMemberDelegate GetPublicGetter(FieldInfo fi) => GetPublicGetter(fi?.Name);

        public virtual GetMemberDelegate GetPublicGetter(string name)
        {
            if (name == null)
                return null;

            return FieldsMap.TryGetValue(name, out FieldAccessor info)
                ? info.PublicGetter
                : null;
        }

        public virtual SetMemberDelegate GetPublicSetter(FieldInfo fi) => GetPublicSetter(fi?.Name);

        public virtual SetMemberDelegate GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            return FieldsMap.TryGetValue(name, out FieldAccessor info)
                ? info.PublicSetter
                : null;
        }

        public virtual SetMemberRefDelegate GetPublicSetterRef(string name)
        {
            if (name == null)
                return null;

            return FieldsMap.TryGetValue(name, out FieldAccessor info)
                ? info.PublicSetterRef
                : null;
        }
    }

    public static class FieldInvoker
    {
        [Obsolete("Use CreateGetter")]
        public static GetMemberDelegate GetFieldGetterFn(this FieldInfo fieldInfo) =>
            PclExport.Instance.CreateGetter(fieldInfo);

        [Obsolete("Use CreateSetter")]
        public static SetMemberDelegate GetFieldSetterFn(this FieldInfo fieldInfo) =>
            PclExport.Instance.CreateSetter(fieldInfo);

        public static GetMemberDelegate CreateGetter(this FieldInfo fieldInfo) =>
            PclExport.Instance.CreateGetter(fieldInfo);

        public static GetMemberDelegate<T> CreateGetter<T>(this FieldInfo fieldInfo) =>
            PclExport.Instance.CreateGetter<T>(fieldInfo);

        public static SetMemberDelegate CreateSetter(this FieldInfo fieldInfo) =>
            PclExport.Instance.CreateSetter(fieldInfo);

        public static SetMemberDelegate<T> CreateSetter<T>(this FieldInfo fieldInfo) =>
            PclExport.Instance.CreateSetter<T>(fieldInfo);

        public static GetMemberDelegate GetReflection(FieldInfo fieldInfo) => fieldInfo.GetValue;
        public static SetMemberDelegate SetReflection(FieldInfo fieldInfo) => fieldInfo.SetValue;

        private static readonly MethodInfo setFieldMethod =
            typeof(FieldInvoker).GetStaticMethod("SetField");

        internal static void SetField<TValue>(ref TValue field, TValue newValue)
        {
            field = newValue;
        }

        public static GetMemberDelegate<T> GetExpression<T>(FieldInfo fieldInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var field = typeof(T) != fieldInfo.DeclaringType
                ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
                : Expression.Field(instance, fieldInfo);
            var convertField = Expression.TypeAs(field, typeof(object));
            return Expression.Lambda<GetMemberDelegate<T>>(convertField, instance).Compile();
        }

        public static GetMemberDelegate GetExpression(FieldInfo fieldInfo)
        {
            var fieldDeclaringType = fieldInfo.DeclaringType;

            var oInstanceParam = Expression.Parameter(typeof(object), "source");
            var instanceParam = GetCastOrConvertExpression(oInstanceParam, fieldDeclaringType);

            var exprCallFieldGetFn = Expression.Field(instanceParam, fieldInfo);
            //var oExprCallFieldGetFn = this.GetCastOrConvertExpression(exprCallFieldGetFn, typeof(object));
            var oExprCallFieldGetFn = Expression.Convert(exprCallFieldGetFn, typeof(object));

            var fieldGetterFn = Expression.Lambda<GetMemberDelegate>
                (
                    oExprCallFieldGetFn,
                    oInstanceParam
                )
                .Compile();

            return fieldGetterFn;
        }

        public static SetMemberDelegate SetExpression(FieldInfo fieldInfo)
        {
            var fieldDeclaringType = fieldInfo.DeclaringType;

            var sourceParameter = Expression.Parameter(typeof(object), "source");
            var valueParameter = Expression.Parameter(typeof(object), "value");

            var sourceExpression = GetCastOrConvertExpression(sourceParameter, fieldDeclaringType);

            var fieldExpression = Expression.Field(sourceExpression, fieldInfo);

            var valueExpression = GetCastOrConvertExpression(valueParameter, fieldExpression.Type);

            var genericSetFieldMethodInfo = setFieldMethod.MakeGenericMethod(fieldExpression.Type);

            var setFieldMethodCallExpression = Expression.Call(
                null, genericSetFieldMethodInfo, fieldExpression, valueExpression);

            var setterFn = Expression.Lambda<SetMemberDelegate>(
                setFieldMethodCallExpression, sourceParameter, valueParameter).Compile();

            return setterFn;
        }

        public static SetMemberDelegate<T> SetExpression<T>(FieldInfo fieldInfo)
        {
            var instance = Expression.Parameter(typeof(T), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var field = typeof(T) != fieldInfo.DeclaringType
                ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
                : Expression.Field(instance, fieldInfo);

            var setterCall = Expression.Assign(
                field,
                Expression.Convert(argument, fieldInfo.FieldType));

            return Expression.Lambda<SetMemberDelegate<T>>
            (
                setterCall, instance, argument
            ).Compile();
        }

        private static Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {
            Expression result;
            var expressionType = expression.Type;

            if (targetType.IsAssignableFrom(expressionType))
            {
                result = expression;
            }
            else
            {
                // Check if we can use the as operator for casting or if we must use the convert method
                if (targetType.IsValueType && !targetType.IsNullableType())
                {
                    result = Expression.Convert(expression, targetType);
                }
                else
                {
                    result = Expression.TypeAs(expression, targetType);
                }
            }

            return result;
        }

        public static SetMemberRefDelegate<T> SetExpressionRef<T>(this FieldInfo fieldInfo)
        {
            var instance = Expression.Parameter(typeof(T).MakeByRefType(), "i");
            var argument = Expression.Parameter(typeof(object), "a");

            var field = typeof(T) != fieldInfo.DeclaringType
                ? Expression.Field(Expression.TypeAs(instance, fieldInfo.DeclaringType), fieldInfo)
                : Expression.Field(instance, fieldInfo);

            var setterCall = Expression.Assign(
                field,
                Expression.Convert(argument, fieldInfo.FieldType));

            return Expression.Lambda<SetMemberRefDelegate<T>>
            (
                setterCall, instance, argument
            ).Compile();
        }

#if NET45 || NETSTANDARD2_0
        public static GetMemberDelegate<T> GetEmit<T>(FieldInfo fieldInfo)
        {
            var getter = CreateDynamicGetMethod<T>(fieldInfo);

            var gen = getter.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);

            gen.Emit(OpCodes.Ldfld, fieldInfo);

            if (fieldInfo.FieldType.IsValueType)
            {
                gen.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            gen.Emit(OpCodes.Ret);

            return (GetMemberDelegate<T>)getter.CreateDelegate(typeof(GetMemberDelegate<T>));
        }

        public static GetMemberDelegate GetEmit(FieldInfo fieldInfo)
        {
            var getter = CreateDynamicGetMethod(fieldInfo);

            var gen = getter.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);

            if (fieldInfo.DeclaringType.IsValueType)
            {
                gen.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }
            else
            {
                gen.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            }

            gen.Emit(OpCodes.Ldfld, fieldInfo);

            if (fieldInfo.FieldType.IsValueType)
            {
                gen.Emit(OpCodes.Box, fieldInfo.FieldType);
            }

            gen.Emit(OpCodes.Ret);

            return (GetMemberDelegate)getter.CreateDelegate(typeof(GetMemberDelegate));
        }

        static readonly Type[] DynamicGetMethodArgs = { typeof(object) };

        internal static DynamicMethod CreateDynamicGetMethod(MemberInfo memberInfo)
        {
            var memberType = memberInfo is FieldInfo ? "Field" : "Property";
            var name = $"_Get{memberType}_{memberInfo.Name}_";
            var returnType = typeof(object);

            return !memberInfo.DeclaringType.IsInterface
                ? new DynamicMethod(name, returnType, DynamicGetMethodArgs, memberInfo.DeclaringType, true)
                : new DynamicMethod(name, returnType, DynamicGetMethodArgs, memberInfo.Module, true);
        }

        internal static DynamicMethod CreateDynamicGetMethod<T>(MemberInfo memberInfo)
        {
            var memberType = memberInfo is FieldInfo ? "Field" : "Property";
            var name = $"_Get{memberType}[T]_{memberInfo.Name}_";
            var returnType = typeof(object);

            return !memberInfo.DeclaringType.IsInterface
                ? new DynamicMethod(name, returnType, new[] { typeof(T) }, memberInfo.DeclaringType, true)
                : new DynamicMethod(name, returnType, new[] { typeof(T) }, memberInfo.Module, true);
        }

        public static SetMemberDelegate SetEmit(FieldInfo fieldInfo)
        {
            var setter = CreateDynamicSetMethod(fieldInfo);

            var gen = setter.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);

            if (fieldInfo.DeclaringType.IsValueType)
            {
                gen.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }
            else
            {
                gen.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            }

            gen.Emit(OpCodes.Ldarg_1);

            gen.Emit(fieldInfo.FieldType.IsClass
                    ? OpCodes.Castclass
                    : OpCodes.Unbox_Any,
                fieldInfo.FieldType);

            gen.Emit(OpCodes.Stfld, fieldInfo);
            gen.Emit(OpCodes.Ret);

            return (SetMemberDelegate)setter.CreateDelegate(typeof(SetMemberDelegate));
        }

        static readonly Type[] DynamicSetMethodArgs = { typeof(object), typeof(object) };

        internal static DynamicMethod CreateDynamicSetMethod(MemberInfo memberInfo)
        {
            var memberType = memberInfo is FieldInfo ? "Field" : "Property";
            var name = $"_Set{memberType}_{memberInfo.Name}_";
            var returnType = typeof(void);

            return !memberInfo.DeclaringType.IsInterface
                ? new DynamicMethod(name, returnType, DynamicSetMethodArgs, memberInfo.DeclaringType, true)
                : new DynamicMethod(name, returnType, DynamicSetMethodArgs, memberInfo.Module, true);
        }
#endif
    }
}
