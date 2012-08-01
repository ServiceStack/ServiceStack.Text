using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace ServiceStack.Text
{
    [Serializable]
    public class DeserializationException : Exception
    {
        public DeserializationException()
        {
        }

        public DeserializationException(string message)
            : base(message)
        {
        }

        public DeserializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected DeserializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
