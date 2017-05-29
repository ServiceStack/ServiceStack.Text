using System;

namespace ServiceStack.Text.TestsConsole
{
    public struct T
    {
        public int PropValue { get; set; }
        public string PropRef { get; set; }

        public int FieldValue;
        public string FieldRef;
    }

    class Program
    {
        static void Main(string[] args)
        {
            var t = new T { PropValue = 1, PropRef = "foo", FieldValue = 2, FieldRef = "bar" };

            //var i1 = GetPropInt(t);
            //var s1 = GetPropString(t);

            //var i2 = GetFieldInt(t);
            //var s2 = GetFieldString(t);

            //$"PropValue: ${i1}, PropRef: ${s1}".Print();
            //$"FieldValue: ${i2}, FieldRef: ${s2}".Print();

            var tuple = ((int i, string s))new ValueTuple<int,string>(1,"foo");

            var oTuple = (object) tuple;
            var value = GetValueTupleItem2(oTuple);

            value.PrintDump();
        }

        static object GetPropInt(object instance)
        {
            var t = (T)instance;
            return t.PropValue;
        }

        static object GetPropString(object instance)
        {
            var t = (T)instance;
            return t.PropRef;
        }

        static object GetFieldInt(object instance)
        {
            var t = (T)instance;
            return t.FieldValue;
        }

        static object GetFieldString(object instance)
        {
            return ((T)instance).FieldRef;
        }

        static object GetValueTupleItem1(object instance)
        {
            return ((ValueTuple<int, string>)instance).Item1;
        }

        static object GetValueTupleItem2(object instance)
        {
            return ((ValueTuple<int, string>)instance).Item2;
        }
    }
}
