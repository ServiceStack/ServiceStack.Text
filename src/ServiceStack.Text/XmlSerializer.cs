#if !LITE
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ServiceStack.Text
{
    public class XmlSerializer
    {
        private static readonly XmlWriterSettings XWSettings = new XmlWriterSettings();
        private static readonly XmlReaderSettings XRSettings = new XmlReaderSettings();

        public static XmlSerializer Instance = new XmlSerializer();

        public XmlSerializer(bool omitXmlDeclaration = false, int maxCharsInDocument = 1024 * 1024)
        {
            XWSettings.Encoding = new UTF8Encoding(false);
            XWSettings.OmitXmlDeclaration = omitXmlDeclaration;
            XRSettings.MaxCharactersInDocument = maxCharsInDocument;
        }

        private static object Deserialize(string xml, Type type)
        {
            try
            {
                var stringReader = new StringReader(xml);
                using (var reader = XmlReader.Create(stringReader, XRSettings))
                {
                    var serializer = new DataContractSerializer(type);
                    return serializer.ReadObject(reader);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException("DeserializeDataContract: Error converting type: " + ex.Message, ex);
            }
        }

        public static object DeserializeFromString(string xml, Type type)
        {
            return Deserialize(xml, type);
        }

        public static T DeserializeFromString<T>(string xml)
        {
            var type = typeof(T);
            return (T)Deserialize(xml, type);
        }

        public static T DeserializeFromReader<T>(TextReader reader)
        {
            return DeserializeFromString<T>(reader.ReadToEnd());
        }

        public static T DeserializeFromStream<T>(Stream stream)
        {
            var serializer = new DataContractSerializer(typeof(T));

            return (T)serializer.ReadObject(stream);
        }

        public static object DeserializeFromStream(Type type, Stream stream)
        {
            var serializer = new DataContractSerializer(type);
            return serializer.ReadObject(stream);
        }

        public static string SerializeToString<T>(T from)
        {
            try
            {
                using (var ms = MemoryStreamFactory.GetStream())
                {
                    using (var xw = XmlWriter.Create(ms, XWSettings))
                    {
                        var serializer = new DataContractSerializer(from.GetType());
                        serializer.WriteObject(xw, from);
                        xw.Flush();
                        ms.Seek(0, SeekOrigin.Begin);
                        var reader = new StreamReader(ms);
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Error serializing object of type {0}", from.GetType().FullName), ex);
            }
        }

        public static void SerializeToWriter<T>(T value, TextWriter writer)
        {
            try
            {
                using (var xw = XmlWriter.Create(writer, XWSettings))
                {
                    var serializer = new DataContractSerializer(value.GetType());
                    serializer.WriteObject(xw, value);
                }
            }
            catch (Exception ex)
            {
                throw new SerializationException(string.Format("Error serializing object of type {0}", value.GetType().FullName), ex);
            }
        }

        public static void SerializeToStream(object obj, Stream stream)
        {
            if (obj == null) return;
            using (var xw = XmlWriter.Create(stream, XWSettings))
            {
                var serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(xw, obj);
            }
        }
    }
}
#endif