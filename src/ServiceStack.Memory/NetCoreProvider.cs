using System;
using System.Buffers.Text;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;
using ServiceStack.Text.Pools;

namespace ServiceStack.Memory
{
    public sealed class NetCoreProvider : MemoryProvider
    {
        private NetCoreProvider(){}
        public static void Configure() => Instance = new NetCoreProvider();
        
        public override bool ParseBoolean(ReadOnlySpan<char> value) => bool.Parse(value);

        public override bool TryParseBoolean(ReadOnlySpan<char> value, out bool result) =>
            bool.TryParse(value, out result);

        public override bool TryParseDecimal(ReadOnlySpan<char> value, out decimal result) =>
            decimal.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);

        public override bool TryParseFloat(ReadOnlySpan<char> value, out float result) =>
            float.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);

        public override bool TryParseDouble(ReadOnlySpan<char> value, out double result) =>
            double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out result);

        public override decimal ParseDecimal(ReadOnlySpan<char> value) =>
            decimal.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);
        
        public override float ParseFloat(ReadOnlySpan<char> value) =>
            float.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        public override double ParseDouble(ReadOnlySpan<char> value) =>
            double.Parse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture);

        public override sbyte ParseSByte(ReadOnlySpan<char> value) => sbyte.Parse(value);

        public override byte ParseByte(ReadOnlySpan<char> value) => byte.Parse(value);

        public override short ParseInt16(ReadOnlySpan<char> value) => short.Parse(value);

        public override ushort ParseUInt16(ReadOnlySpan<char> value) => ushort.Parse(value);

        public override int ParseInt32(ReadOnlySpan<char> value) => int.Parse(value);

        public override uint ParseUInt32(ReadOnlySpan<char> value) => uint.Parse(value);

        public override long ParseInt64(ReadOnlySpan<char> value) => long.Parse(value);

        public override ulong ParseUInt64(ReadOnlySpan<char> value) => ulong.Parse(value);

        public override Guid ParseGuid(ReadOnlySpan<char> value) => Guid.Parse(value);
        
        public override byte[] ParseBase64(ReadOnlySpan<char> value)
        {
            byte[] bytes = BufferPool.GetBuffer(Base64.GetMaxDecodedFromUtf8Length(value.Length));
            try
            {
                if (Convert.TryFromBase64Chars(value, bytes, out var bytesWritten))
                {
                    var ret = new byte[bytesWritten];
                    Buffer.BlockCopy(bytes, 0, ret, 0, bytesWritten);
                    return ret;
                }
                else
                {
                    var chars = value.ToArray();
                    return Convert.FromBase64CharArray(chars, 0, chars.Length);
                }
            }
            finally 
            {
                BufferPool.ReleaseBufferToPool(ref bytes);
            }
        }

        public override Task WriteAsync(Stream stream, ReadOnlySpan<char> value, CancellationToken token=default)
        {
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen:true))
            {
                writer.Write(value);
            }

            return TypeConstants.EmptyTask;
        }

        public override async Task<object> DeserializeAsync(Type type, Stream stream, TypeDeserializer deserializer)
        {
            //TODO optimize Stream -> UTF-8 ReadOnlySpan<char>

            if (stream is MemoryStream ms)
            {
                var body = await ms.ReadToEndAsync(Encoding.UTF8);
                var ret = deserializer(type, body.AsSpan());
                return ret;
            }

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            
            using (var reader = new StreamReader(stream, Encoding.UTF8, true, StreamExtensions.DefaultBufferSize, leaveOpen:true))
            {
                var body = await reader.ReadToEndAsync();
                var ret = deserializer(type, body.AsSpan());
                return ret;
            }
        }

        public override StringBuilder Append(StringBuilder sb, ReadOnlySpan<char> value)
        {
            return sb.Append(value);
        }
    }    
}