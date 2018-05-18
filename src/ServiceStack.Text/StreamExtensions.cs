//Copyright (c) ServiceStack, Inc. All Rights Reserved.
//License: https://raw.github.com/ServiceStack/ServiceStack/master/license.txt

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack.Text;

namespace ServiceStack
{
    public static class StreamExtensions
    {
        public static long WriteTo(this Stream inStream, Stream outStream)
        {
            var memoryStream = inStream as MemoryStream;
            if (memoryStream != null)
            {
                memoryStream.WriteTo(outStream);
                return memoryStream.Position;
            }

            var data = new byte[4096];
            long total = 0;
            int bytesRead;

            while ((bytesRead = inStream.Read(data, 0, data.Length)) > 0)
            {
                outStream.Write(data, 0, bytesRead);
                total += bytesRead;
            }

            return total;
        }

        public static IEnumerable<string> ReadLines(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            using (var reader = new StreamReader(stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        /// <summary>
        /// @jonskeet: Collection of utility methods which operate on streams.
        /// r285, February 26th 2009: http://www.yoda.arachsys.com/csharp/miscutil/
        /// </summary>
        const int DefaultBufferSize = 8 * 1024;

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte
        /// array.
        /// </summary>
        public static byte[] ReadFully(this Stream input)
        {
            return ReadFully(input, DefaultBufferSize);
        }

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte
        /// array, using the given buffer size.
        /// </summary>
        public static byte[] ReadFully(this Stream input, int bufferSize)
        {
            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            return ReadFully(input, new byte[bufferSize]);
        }

        /// <summary>
        /// Reads the given stream up to the end, returning the data as a byte
        /// array, using the given buffer for transferring data. Note that the
        /// current contents of the buffer is ignored, so the buffer needn't
        /// be cleared beforehand.
        /// </summary>
        public static byte[] ReadFully(this Stream input, byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (buffer.Length == 0)
                throw new ArgumentException("Buffer has length of 0");

            // We could do all our own work here, but using MemoryStream is easier
            // and likely to be just as efficient.
            using (var tempStream = MemoryStreamFactory.GetStream())
            {
                CopyTo(input, tempStream, buffer);
                // No need to copy the buffer if it's the right size
                if (tempStream.Length == tempStream.GetBuffer().Length)
                {
                    return tempStream.GetBuffer();
                }
                // Okay, make a copy that's the right size
                return tempStream.ToArray();
            }
        }

        /// <summary>
        /// Copies all the data from one stream into another.
        /// </summary>
        public static long CopyTo(this Stream input, Stream output)
        {
            return CopyTo(input, output, DefaultBufferSize);
        }

        /// <summary>
        /// Copies all the data from one stream into another, using a buffer
        /// of the given size.
        /// </summary>
        public static long CopyTo(this Stream input, Stream output, int bufferSize)
        {
            if (bufferSize < 1)
                throw new ArgumentOutOfRangeException(nameof(bufferSize));

            return CopyTo(input, output, new byte[bufferSize]);
        }

        /// <summary>
        /// Copies all the data from one stream into another, using the given 
        /// buffer for transferring data. Note that the current contents of 
        /// the buffer is ignored, so the buffer needn't be cleared beforehand.
        /// </summary>
        public static long CopyTo(this Stream input, Stream output, byte[] buffer)
        {
            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (output == null)
                throw new ArgumentNullException(nameof(output));

            if (buffer.Length == 0)
                throw new ArgumentException("Buffer has length of 0");

            long total = 0;
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, read);
                total += read;
            }
            return total;
        }

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream.
        /// If the end of the stream is reached before the specified amount
        /// of data is read, an exception is thrown.
        /// </summary>
        public static byte[] ReadExactly(this Stream input, int bytesToRead)
        {
            return ReadExactly(input, new byte[bytesToRead]);
        }

        /// <summary>
        /// Reads into a buffer, filling it completely.
        /// </summary>
        public static byte[] ReadExactly(this Stream input, byte[] buffer)
        {
            return ReadExactly(input, buffer, buffer.Length);
        }

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream,
        /// into the given buffer, starting at position 0 of the array.
        /// </summary>
        public static byte[] ReadExactly(this Stream input, byte[] buffer, int bytesToRead)
        {
            return ReadExactly(input, buffer, 0, bytesToRead);
        }

        /// <summary>
        /// Reads exactly the given number of bytes from the specified stream,
        /// into the given buffer, starting at position 0 of the array.
        /// </summary>
        public static byte[] ReadExactly(this Stream input, byte[] buffer, int startIndex, int bytesToRead)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            if (buffer == null)
                throw new ArgumentNullException(nameof(buffer));

            if (startIndex < 0 || startIndex >= buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(startIndex));

            if (bytesToRead < 1 || startIndex + bytesToRead > buffer.Length)
                throw new ArgumentOutOfRangeException(nameof(bytesToRead));

            return ReadExactlyFast(input, buffer, startIndex, bytesToRead);
        }

        /// <summary>
        /// Same as ReadExactly, but without the argument checks.
        /// </summary>
        private static byte[] ReadExactlyFast(Stream fromStream, byte[] intoBuffer, int startAtIndex, int bytesToRead)
        {
            var index = 0;
            while (index < bytesToRead)
            {
                var read = fromStream.Read(intoBuffer, startAtIndex + index, bytesToRead - index);
                if (read == 0)
                    throw new EndOfStreamException
                        ($"End of stream reached with {bytesToRead - index} byte{(bytesToRead - index == 1 ? "s" : "")} left to read.");

                index += read;
            }
            return intoBuffer;
        }

        public static string CollapseWhitespace(this string str)
        {
            if (str == null)
                return null;

            var sb = StringBuilderThreadStatic.Allocate();
            var lastChar = (char)0;
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (c < 32) continue; // Skip all these
                if (c == 32)
                {
                    if (lastChar == 32)
                        continue; // Only write one space character
                }
                sb.Append(c);
                lastChar = c;
            }

            return StringBuilderThreadStatic.ReturnAndFree(sb);
        }

        public static byte[] Combine(this byte[] bytes, params byte[][] withBytes)
        {
            var combinedLength = bytes.Length + withBytes.Sum(b => b.Length);
            var to = new byte[combinedLength];

            Buffer.BlockCopy(bytes, 0, to, 0, bytes.Length);
            var pos = bytes.Length;

            foreach (var b in withBytes)
            {
                Buffer.BlockCopy(b, 0, to, pos, b.Length);
                pos += b.Length;
            }

            return to;
        }

        public static int AsyncBufferSize = 81920; // CopyToAsync() default value

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WriteAsync(this Stream stream, byte[] bytes, CancellationToken token = default(CancellationToken)) => stream.WriteAsync(bytes, 0, bytes.Length, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task CopyToAsync(this Stream input, Stream output, CancellationToken token = default(CancellationToken)) => input.CopyToAsync(output, AsyncBufferSize, token);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Task WriteAsync(this Stream stream, string text, CancellationToken token = default(CancellationToken)) => stream.WriteAsync(text.ToUtf8Bytes(), token);

        public static string ToMd5Hash(this Stream stream)
        {
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(stream);
            var sb = StringBuilderCache.Allocate();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return StringBuilderCache.ReturnAndFree(sb);
        }

        public static string ToMd5Hash(this byte[] bytes)
        {
            var hash = System.Security.Cryptography.MD5.Create().ComputeHash(bytes);
            var sb = StringBuilderCache.Allocate();
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return StringBuilderCache.ReturnAndFree(sb);
        }
        
    }
}