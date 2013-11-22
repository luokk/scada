using System;
using System.Runtime.Serialization;

namespace Scada.Common
{
    public class InterProcessIOException : Exception
    {
        public bool IsServerAvailable = true;

        public uint ErrorCode = 0;

        public InterProcessIOException(String text)
            : base(text)
        {
        }

        protected InterProcessIOException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
