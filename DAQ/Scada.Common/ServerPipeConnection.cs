using System;
using System.IO;

namespace Scada.Common
{
    public sealed class ServerPipeConnection : PipeConnection
    {
        public ServerPipeConnection(string name, uint outBuffer, uint inBuffer, int maxReadBytes)
        {
            this.Name = name;
            this.Handle = NamedPipeWrapper.Create(name, outBuffer, inBuffer, true);
            this.maxReadBytes = maxReadBytes;
        }

        public ServerPipeConnection(string name, uint outBuffer, uint inBuffer, int maxReadBytes, bool secure)
        {
            this.Name = name;
            this.Handle = NamedPipeWrapper.Create(name, outBuffer, inBuffer, secure);
            this.maxReadBytes = maxReadBytes;
        }

        ~ServerPipeConnection()
        {
            Dispose(false);
        }
        
        public void Disconnect()
        {
            DisposedCheck();
            NamedPipeWrapper.Disconnect(this.Handle);
        }

        public override void Close()
        {
            DisposedCheck();
            NamedPipeWrapper.Close(this.Handle);
        }

        public override void Connect()
        {
            DisposedCheck();
            NamedPipeWrapper.Connect(this.Handle);
        }

    }
}