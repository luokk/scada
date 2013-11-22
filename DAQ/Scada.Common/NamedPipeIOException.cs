using System;
using System.Runtime.Serialization;

namespace Scada.Common
{

    public class NamedPipeIOException : InterProcessIOException
    {

        public NamedPipeIOException(String text)
            : base(text)
        {
        }

        public NamedPipeIOException(String text, uint errorCode)
            : base(text)
        {
            this.ErrorCode = errorCode;
            if (errorCode == NamedPipeNative.ERROR_CANNOT_CONNECT_TO_PIPE)
            {
                this.IsServerAvailable = false;
            }
        }

        protected NamedPipeIOException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}