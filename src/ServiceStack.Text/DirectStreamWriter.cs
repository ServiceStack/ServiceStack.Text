using System;
using System.IO;
using System.Text;

namespace ServiceStack.Text
{
    public class DirectStreamWriter : TextWriter
    {
        private const int optimizedBufferLength = 256;
        private const int maxBufferLength = 1024;
        
        private Stream stream;
        private StreamWriter writer = null;
        private char[] curChar = new char[1];

        private Encoding encoding;
        public override Encoding Encoding => encoding;

        public DirectStreamWriter(Stream stream, Encoding encoding)
        {
            this.stream = stream;
            this.encoding = encoding;
        }

        public override void Write(string s)
        {
            if (s.Length <= optimizedBufferLength)
            {
                byte[] buffer = Encoding.GetBytes(s);
                stream.Write(buffer, 0, buffer.Length);
            } 
            else 
            {
                if (writer == null)
                    writer = new StreamWriter(stream, Encoding, s.Length < maxBufferLength ? s.Length : maxBufferLength);
                
                writer.Write(s);
                writer.Flush();
            }
        }

        public override void Write(char c)
        {
            curChar[0] = c;

            byte[] buffer = Encoding.GetBytes(curChar);
            stream.Write(buffer, 0, buffer.Length);
        }

        public override void Flush()
        {
            if (writer != null)
            {
                writer.Flush();
            }
            else
            {
                stream.Flush();
            }
        }
    }
}