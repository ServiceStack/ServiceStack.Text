
#if !XBOX360 && !SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH
using System.IO.Compression;
#endif

using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace ServiceStack.Text
{
#if !XBOX
    public class XmlSerializer
    {
        private readonly XmlDictionaryReaderQuotas quotas;
        private static readonly XmlWriterSettings XSettings = new XmlWriterSettings();

        public static XmlSerializer Instance
            = new XmlSerializer(
#if !SILVERLIGHT && !WINDOWS_PHONE && !MONOTOUCH
                new XmlDictionaryReaderQuotas { MaxStringContentLength = 1024 * 1024, }
#endif
);

        public XmlSerializer(XmlDictionaryReaderQuotas quotas=null, bool omitXmlDeclaration = false)
        {
            this.quotas = quotas;
            XSettings.Encoding = new UTF8Encoding(false);
            XSettings.OmitXmlDeclaration = omitXmlDeclaration;
        }

        private static object Deserialize(string xml, Type type, XmlDictionaryReaderQuotas quotas)
        {
            try
            {
#if WINDOWS_PHONE
                StringReader stringReader = new StringReader(xml);
                using (var reader = XmlDictionaryReader.Create(stringReader))
                {
                    var serializer = new DataContractSerializer(type);
                    return serializer.ReadObject(reader);
                }
#else
                var bytes = Encoding.UTF8.GetBytes(xml);
                using (var reader = XmlDictionaryReader.CreateTextReader(bytes, quotas))
                {
                    var serializer = new DataContractSerializer(type);
                    return serializer.ReadObject(reader);
                }
#endif
            }
            catch (Exception ex)
            {
                throw new SerializationException("DeserializeDataContract: Error converting type: " + ex.Message, ex);
            }
        }

        public static object DeserializeFromString(string xml, Type type)
        {
            return Deserialize(xml, type, Instance.quotas);
        }

        public static T DeserializeFromString<T>(string xml)
        {
            var type = typeof(T);
            return (T)Deserialize(xml, type, Instance.quotas);
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
                using (var ms = new MemoryStream())
                {
                    using (var xw = XmlWriter.Create(ms, XSettings))
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
#if !SILVERLIGHT
				using (var xw = new XmlTextWriter(writer))
#else
                using (var xw = XmlWriter.Create(writer))
#endif
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
#if !SILVERLIGHT
            using (var xw = new XmlTextWriter(stream, Encoding.UTF8))
#else
            using (var xw = XmlWriter.Create(stream))
#endif
            {
                var serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(xw, obj);
            }
        }


#if !SILVERLIGHT && !MONOTOUCH
        public static void CompressToStream<TXmlDto>(TXmlDto from, Stream stream)
        {
            using (var deflateStream = new DeflateStream(stream, CompressionMode.Compress))
            using (var xw = new XmlTextWriter(deflateStream, Encoding.UTF8))
            {
                var serializer = new DataContractSerializer(from.GetType());
                serializer.WriteObject(xw, from);
                xw.Flush();
            }
        }

        public static byte[] Compress<TXmlDto>(TXmlDto from)
        {
            using (var ms = new MemoryStream())
            {
                CompressToStream(from, ms);

                return ms.ToArray();
            }
        }
#endif

    }
#endif
}
