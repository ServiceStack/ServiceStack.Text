using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using ServiceStack.Common.Tests;
using ServiceStack.Reflection;

namespace ServiceStack.Text.TestsConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //var da = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("dyn"),  AssemblyBuilderAccess.Save);

            //var dm = da.DefineDynamicModule("dyn_mod", "dyn.dll");
            //var dt = dm.DefineType("dyn_type");

            //var type = typeof(KeyValuePair<string,string>);
            //var pi = type.GetProperty("Key");

            //var lambdaValueType = PropertyInvoker.GetExpressionLambda<KeyValuePair<string,string>>(pi);
            //lambdaValueType.CompileToMethod(dt.DefineMethod("KVP", MethodAttributes.Public | MethodAttributes.Static));

            //var lambdaRefType = PropertyInvoker.GetExpressionLambda<TRef>(typeof(TRef).GetProperty("PropRef"));
            //lambdaRefType.CompileToMethod(dt.DefineMethod("TRef_PropRef", MethodAttributes.Public | MethodAttributes.Static));

            //var lambdaRefType2 = PropertyInvoker.GetExpressionLambda<IncludeExclude>(typeof(IncludeExclude).GetProperty("Id"));
            //lambdaRefType2.CompileToMethod(dt.DefineMethod("IncludeExclude_Id", MethodAttributes.Public | MethodAttributes.Static));

            //dt.CreateType();
            //da.Save("dyn.dll");

            new StringConcatPerfTests {
                MultipleIterations = new[] { 1000, 10000, 100000, 1000000, 10000000 }
            }.Compare_interpolation_vs_string_Concat();

            Console.ReadLine();
        }

        class StringConcatPerfTests : PerfTestBase
        {
            public void Compare_interpolation_vs_string_Concat()
            {
                CompareMultipleRuns(
                    "Interpolation",
                    () => SimpleInterpolation("foo"),
                    "string.Concat",
                    () => SimpleConcat("foo"));
            }
        }


        public static object SimpleInterpolation(string text) => $"Hi {text}";

        public static object SimpleFormat(string text) => string.Format("Hi {0}", text);

        public static object SimpleConcat(string text) => "Hi " + text;
    }
}
