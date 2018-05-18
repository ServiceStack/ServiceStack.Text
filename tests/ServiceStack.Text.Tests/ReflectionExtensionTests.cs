﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ServiceStack;

namespace ServiceStack.Text.Tests
{
    public class TestModel
    {
        public TestModel()
        {
            var i = 0;
            this.PublicInt = i++;
            this.PublicGetInt = i++;
            this.PublicSetInt = i++;
            this.PublicIntField = i++;
            this.PrivateInt = i++;
            this.ProtectedInt = i++;
        }

        public int PublicInt { get; set; }

        public int PublicGetInt { get; private set; }

        public int PublicSetInt { private get; set; }

        public int PublicIntField;

        private int PrivateInt { get; set; }

        protected int ProtectedInt { get; set; }

        public int IntMethod()
        {
            return this.PublicInt;
        }
    }

    public class MethodsForReflection
    {
        public string Result = String.Empty;

        public void HelloVoid()
        {
            Result = "Hello";
        }

        public void Hello(bool a, int b)
        {
            Result = String.Format($"Hello {a} {b}");
        }
    }

    [TestFixture]
    public class ReflectionExtensionTests
        : TestBase
    {

        [Test]
        public void Only_serializes_public_readable_properties()
        {
            var model = new TestModel();
            var modelStr = TypeSerializer.SerializeToString(model);

            Assert.That(modelStr, Is.EqualTo("{PublicInt:0,PublicGetInt:1}"));

            Serialize(model);
        }

        [Test]
        public void Can_create_instance_of_string()
        {
            Assert.That(typeof(string).CreateInstance(), Is.EqualTo(String.Empty));
        }

        [Test]
        public void Can_create_instances_of_common_collections()
        {
            Assert.That(typeof(IEnumerable<TestModel>).CreateInstance() as IEnumerable<TestModel>, Is.Not.Null);
            Assert.That(typeof(ICollection<TestModel>).CreateInstance() as ICollection<TestModel>, Is.Not.Null);
            Assert.That(typeof(IList<TestModel>).CreateInstance() as IList<TestModel>, Is.Not.Null);
            Assert.That(typeof(IDictionary<string, TestModel>).CreateInstance() as IDictionary<string, TestModel>, Is.Not.Null);
            Assert.That(typeof(IDictionary<int, TestModel>).CreateInstance() as IDictionary<int, TestModel>, Is.Not.Null);
            Assert.That(typeof(TestModel[]).CreateInstance() as TestModel[], Is.Not.Null);
        }

        [Test]
        public void Can_create_intances_of_generic_types()
        {
            Assert.That(typeof(GenericType<>).CreateInstance(), Is.Not.Null);
            Assert.That(typeof(GenericType<,>).CreateInstance(), Is.Not.Null);
            Assert.That(typeof(GenericType<,,>).CreateInstance(), Is.Not.Null);
            Assert.That(typeof(GenericType<GenericType<object>>).CreateInstance(), Is.Not.Null);
        }

        [Test]
        public void Can_create_intances_of_recursive_generic_type()
        {
            //Assert.That(typeof(GenericType<>).MakeGenericType(new[] { typeof(GenericType<>) }).CreateInstance(), Is.Not.Null);
        }

        [Test]
        public void Can_get_method_from_type()
        {
            var testInstance = new MethodsForReflection();

            var helloVoidMethod = typeof(MethodsForReflection).GetMethodInfo(nameof(MethodsForReflection.HelloVoid));
            Assert.That(helloVoidMethod, Is.Not.Null);
            var helloVoidDelegate = (Action<MethodsForReflection>)helloVoidMethod.MakeDelegate(typeof(Action<MethodsForReflection>));
            Assert.That(helloVoidDelegate, Is.Not.Null);
            helloVoidDelegate(testInstance);
            Assert.That(testInstance.Result, Is.EqualTo("Hello"));

            var helloVoidBoolIntMethod = typeof(MethodsForReflection).GetMethodInfo(nameof(MethodsForReflection.Hello), new Type[] { typeof(bool), typeof(int) });
            Assert.That(helloVoidBoolIntMethod, Is.Not.Null);
            var helloVoidBoolIntDelegate = (Action<MethodsForReflection, bool, int>)helloVoidBoolIntMethod.MakeDelegate(typeof(Action<MethodsForReflection, bool, int>));
            Assert.That(helloVoidBoolIntDelegate, Is.Not.Null);
            helloVoidBoolIntDelegate(testInstance, true, 5);
            Assert.That(testInstance.Result, Is.EqualTo("Hello True 5"));
        }

        [Test]
        public void Does_GetCollectionType()
        {
            Assert.That(new[] { new TestModel() }.GetType().GetCollectionType(), Is.EqualTo(typeof(TestModel)));
            Assert.That(new[] { new TestModel() }.ToList().GetType().GetCollectionType(), Is.EqualTo(typeof(TestModel)));
            Assert.That(new[] { new TestModel() }.Select(x => x).GetType().GetCollectionType(), Is.EqualTo(typeof(TestModel)));
            Assert.That(new[] { "" }.Select(x => new TestModel()).GetType().GetCollectionType(), Is.EqualTo(typeof(TestModel)));
        }

    }

    public class GenericType<T> { }
    public class GenericType<T1, T2> { }
    public class GenericType<T1, T2, T3> { }
}
