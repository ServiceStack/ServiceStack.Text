using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using ServiceStack.Reflection;

namespace ServiceStack.Text.TestsConsole
{
    public struct TValue
    {
        public int PropValue { get; set; }
        public string PropRef { get; set; }

        public int FieldValue;
        public string FieldRef;
    }

    public class TRef
    {
        public int PropValue { get; set; }
        public string PropRef { get; set; }

        public int FieldValue;
        public string FieldRef;
    }

    class Program
    {
        public class IncludeExclude
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        static void Main(string[] args)
        {
            var da = AppDomain.CurrentDomain.DefineDynamicAssembly(
                new AssemblyName("dyn"), // call it whatever you want
                AssemblyBuilderAccess.Save);

            var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
            var dt = dm.DefineType("dyn_type");


            var type = typeof(KeyValuePair<string,string>);
            var pi = type.GetProperty("Key");

            var lambdaValueType = PropertyInvoker.GetExpressionLambda<KeyValuePair<string,string>>(pi);
            lambdaValueType.CompileToMethod(dt.DefineMethod("KVP", MethodAttributes.Public | MethodAttributes.Static));

            var lambdaRefType = PropertyInvoker.GetExpressionLambda<TRef>(typeof(TRef).GetProperty("PropRef"));
            lambdaRefType.CompileToMethod(dt.DefineMethod("TRef_PropRef", MethodAttributes.Public | MethodAttributes.Static));

            var lambdaRefType2 = PropertyInvoker.GetExpressionLambda<IncludeExclude>(typeof(IncludeExclude).GetProperty("Id"));
            lambdaRefType2.CompileToMethod(dt.DefineMethod("IncludeExclude_Id", MethodAttributes.Public | MethodAttributes.Static));


            dt.CreateType();
            da.Save("dyn.dll");
        }

        static object GetPropInt(object instance)
        {
            var t = (TValue)instance;
            return t.PropValue;
        }

        static object GetPropString(object instance)
        {
            var t = (TValue)instance;
            return t.PropRef;
        }

        static object GetFieldInt(object instance)
        {
            var t = (TValue)instance;
            return t.FieldValue;
        }

        static object GetFieldString(object instance)
        {
            return ((TValue)instance).FieldRef;
        }

        static object GetValueTupleItem1(object instance)
        {
            return ((ValueTuple<int, string>)instance).Item1;
        }

        static object GetValueTupleItem2(object instance)
        {
            return ((ValueTuple<int, string>)instance).Item2;
        }

        static object GetKVP(KeyValuePair<string, string> entry)
        {
            return entry.Key;
        }

        static object GetKVPT<K,V>(KeyValuePair<K,V> entry)
        {
            return entry.Key;
        }
    }
}
