using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using ServiceStack.Reflection;
using ServiceStack.Text;

#if !NETSTANDARD1_1 || NETSTANDARD1_3
using System.Linq.Expressions;
#endif

#if NET45 || NETSTANDARD1_3
using System.Reflection.Emit;
#endif

namespace ServiceStack
{
    public class TypeFieldInfo
    {
        public TypeFieldInfo(
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
                    var fnRef = fi.GetValueSetterGenericRef<T>();
                    Instance.FieldsMap[fi.Name] = new TypeFieldInfo(
                        fi,
                        PclExport.Instance.GetFieldGetterFn(fi),
                        PclExport.Instance.GetFieldSetterFn(fi),
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

        public Type Type { get; protected set; }

        public readonly Dictionary<string, TypeFieldInfo> FieldsMap =
            new Dictionary<string, TypeFieldInfo>(PclExport.Instance.InvariantComparerIgnoreCase);

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

            return FieldsMap.TryGetValue(name, out TypeFieldInfo info)
                ? info.PublicGetter
                : null;
        }

        public virtual SetMemberDelegate GetPublicSetter(FieldInfo fi) => GetPublicSetter(fi?.Name);

        public virtual SetMemberDelegate GetPublicSetter(string name)
        {
            if (name == null)
                return null;

            return FieldsMap.TryGetValue(name, out TypeFieldInfo info)
                ? info.PublicSetter
                : null;
        }

        public virtual SetMemberRefDelegate GetPublicSetterRef(string name)
        {
            if (name == null)
                return null;

            return FieldsMap.TryGetValue(name, out TypeFieldInfo info)
                ? info.PublicSetterRef
                : null;
        }
    }

    public static class FieldInvoker
    {
        public static GetMemberDelegate GetFieldGetterFn(this FieldInfo fieldInfo) =>
            PclExport.Instance.GetFieldGetterFn(fieldInfo);

        public static SetMemberDelegate GetFieldSetterFn(this FieldInfo fieldInfo) =>
            PclExport.Instance.GetFieldSetterFn(fieldInfo);

        public static GetMemberDelegate GetReflection(FieldInfo fieldInfo) => fieldInfo.GetValue;
        public static SetMemberDelegate SetReflection(FieldInfo fieldInfo) => fieldInfo.SetValue;

        private static readonly MethodInfo setFieldMethod =
            typeof(FieldInvoker).GetStaticMethod("SetField");

        internal static void SetField<TValue>(ref TValue field, TValue newValue)
        {
            field = newValue;
        }

#if (!NETSTANDARD1_1 || NETSTANDARD1_3)
        public static GetMemberDelegate GetExpression(FieldInfo fieldInfo)
        {
            try
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
            catch (Exception ex)
            {
                Tracer.Instance.WriteError(ex);
                throw;
            }
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

        private static Expression GetCastOrConvertExpression(Expression expression, Type targetType)
        {
            Expression result;
            var expressionType = expression.Type;

            if (targetType.IsAssignableFromType(expressionType))
            {
                result = expression;
            }
            else
            {
                // Check if we can use the as operator for casting or if we must use the convert method
                if (targetType.IsValueType() && !targetType.IsNullableType())
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
#endif

#if NET45 || NETSTANDARD1_3
        public static GetMemberDelegate GetEmit(FieldInfo fieldInfo)
        {
            var getter = CreateDynamicGetMethod(fieldInfo);

            var gen = getter.GetILGenerator();

            gen.Emit(OpCodes.Ldarg_0);

            if (fieldInfo.DeclaringType.IsValueType())
            {
                gen.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }
            else
            {
                gen.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            }

            gen.Emit(OpCodes.Ldfld, fieldInfo);

            if (fieldInfo.FieldType.IsValueType())
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

            return !memberInfo.DeclaringType.IsInterface()
                ? new DynamicMethod(name, returnType, DynamicGetMethodArgs, memberInfo.DeclaringType, true)
                : new DynamicMethod(name, returnType, DynamicGetMethodArgs, memberInfo.Module, true);
        }

        public static SetMemberDelegate SetEmit(FieldInfo fieldInfo)
        {
            var setter = CreateDynamicSetMethod(fieldInfo);

            var gen = setter.GetILGenerator();
            gen.Emit(OpCodes.Ldarg_0);

            if (fieldInfo.DeclaringType.IsValueType())
            {
                gen.Emit(OpCodes.Unbox, fieldInfo.DeclaringType);
            }
            else
            {
                gen.Emit(OpCodes.Castclass, fieldInfo.DeclaringType);
            }

            gen.Emit(OpCodes.Ldarg_1);

            gen.Emit(fieldInfo.FieldType.IsClass()
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

            return !memberInfo.DeclaringType.IsInterface()
                ? new DynamicMethod(name, returnType, DynamicSetMethodArgs, memberInfo.DeclaringType, true)
                : new DynamicMethod(name, returnType, DynamicSetMethodArgs, memberInfo.Module, true);
        }
#endif

        }
}