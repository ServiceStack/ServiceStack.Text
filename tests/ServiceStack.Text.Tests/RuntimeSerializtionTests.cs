using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using NUnit.Framework;
using ServiceStack.Auth;
using ServiceStack.Messaging;

namespace ServiceStack.Text.Tests
{
    public class RuntimeObject
    {
        public object Object { get; set; }
    }

    public class AType {}

    [DataContract]
    public class DtoType { }

    [RuntimeSerializable]
    public class RuntimeSerializableType { }

    public class MetaType : IMeta
    {
        public Dictionary<string, string> Meta { get; set; }
    }

    public class RequestDto : IReturn<RequestDto> {}

#if NET45
    [Serializable]
    public class SerialiazableType { }
#endif

    public class RuntimeInterface
    {
        public IObject Object { get; set; }
    }
    public interface IObject { }
    public class AInterface : IObject { }

    public class RuntimeObjects
    {
        public object[] Objects { get; set; }
    }

    public class RuntimeSerializtionTests
    {
        string CreateJson(Type type) => CreateJson(type.AssemblyQualifiedName);
        string CreateJson(string typeInfo) => "{\"Object\":{\"__type\":\"" + typeInfo + "\"}}";

        [Test]
        public void Does_allow_builtin_DataTypes_in_Object()
        {
            var dto = "{\"Object\":1}".FromJson<RuntimeObject>();
            Assert.That(dto.Object, Is.EqualTo("1"));

            dto = "{\"Object\":\"foo\"}".FromJson<RuntimeObject>();
            Assert.That(dto.Object, Is.EqualTo("foo"));
        }

        [Test]
        public void Does_not_allow_UnknownTypes_in_Object()
        {
            var types = new[]
            {
                typeof(AType),
                typeof(XmlReaderSettings)
            };

            foreach (var type in types)
            {
                var json = CreateJson(type);
                try
                {
                    var instance = json.FromJson<RuntimeObject>();
                    Assert.Fail("Should throw " + type.Name);
                }
                catch (NotSupportedException ex)
                {
                    ex.Message.Print();
                }
            }
        }

        [Test]
        public void Can_bypass_RuntimeType_validation()
        {
            JsConfig.AllowRuntimeType = type => true;

            var json = CreateJson(typeof(AType));
            var instance = json.FromJson<RuntimeObject>();
            Assert.That(instance.Object, Is.TypeOf<AType>());

            JsConfig.AllowRuntimeType = null;
        }

        [Test]
        public void Does_Serialize_Allowed_Types()
        {
            var allowTypes = new[]
            {
                typeof(DtoType),
                typeof(RuntimeSerializableType),
                typeof(MetaType),
                typeof(RequestDto),
                typeof(UserAuth),
                typeof(UserAuthDetails),
                typeof(Message),
#if NET45
                typeof(SerialiazableType),
#endif
            };

            foreach (var allowType in allowTypes)
            {
                var json = CreateJson(allowType);
                var instance = json.FromJson<RuntimeObject>();
                Assert.That(instance.Object.GetType(), Is.EqualTo(allowType));
            }
        }

        [Test]
        public void Does_allow_Unknown_Types_in_Interface()
        {
            var allowTypes = new[]
            {
                typeof(AInterface),
            };

            foreach (var allowType in allowTypes)
            {
                var json = CreateJson(allowType);
                var instance = json.FromJson<RuntimeInterface>();
                Assert.That(instance.Object.GetType(), Is.EqualTo(allowType));
            }
        }

        string CreateJsonArray(Type type) => CreateJsonArray(type.AssemblyQualifiedName);
        string CreateJsonArray(string typeInfo) => "{\"Objects\":[{\"__type\":\"" + typeInfo + "\"}]}";

        [Test]
        public void Does_not_allow_UnknownTypes_in_Objects_Array()
        {
            var types = new[]
            {
                typeof(AType),
                typeof(XmlReaderSettings)
            };

            foreach (var type in types)
            {
                var json = CreateJsonArray(type);
                try
                {
                    var instance = json.FromJson<RuntimeObjects>();
                    Assert.Fail("Should throw " + type.Name);
                }
                catch (NotSupportedException ex)
                {
                    ex.Message.Print();
                }
            }
        }

        [Test]
        public void Does_Serialize_Allowed_Types_in_Objects_Array()
        {
            var allowTypes = new[]
            {
                typeof(DtoType),
                typeof(RuntimeSerializableType),
                typeof(MetaType),
                typeof(RequestDto),
                typeof(UserAuth),
                typeof(UserAuthDetails),
                typeof(Message),
#if NET45
                typeof(SerialiazableType),
#endif
            };

            foreach (var allowType in allowTypes)
            {
                var json = CreateJsonArray(allowType);
                var instance = json.FromJson<RuntimeObjects>();
                Assert.That(instance.Objects.Length, Is.EqualTo(1));
                Assert.That(instance.Objects[0].GetType(), Is.EqualTo(allowType));
            }
        }

        [Test]
        public void Does_allow_Unknown_Type_in_MQ_Messages()
        {
            //Uses JsConfig.AllowRuntimeTypeInTypesWithNamespaces

            var mqMessage = new Message<AType> //Internal Type used for ServiceStack MQ
            {
                Body = new AType()
            };

            var json = mqMessage.ToJson();

            var fromJson = json.FromJson<Message>();
            Assert.That(fromJson.Body.GetType(), Is.EqualTo(typeof(AType)));
        }
    }
}