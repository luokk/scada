using System;
using System.IO;

namespace Scada.Common
{
    public sealed class ClientPipeConnection : PipeConnection
    {
        private string Server = ".";

        public ClientPipeConnection()
        {
        }

        public ClientPipeConnection(string name)
        {
            this.Name = name;
            this.Server = ".";
            this.maxReadBytes = Int32.MaxValue;
        }

        public ClientPipeConnection(string name, string server)
        {
            this.Name = name;
            this.Server = server;
            this.maxReadBytes = Int32.MaxValue;
        }

        ~ClientPipeConnection()
        {
            Dispose(false);
        }

        public override void Close()
        {
            DisposedCheck();
            NamedPipeWrapper.Close(this.Handle);
        }

        public override void Connect()
        {
            DisposedCheck();
            this.Handle = NamedPipeWrapper.ConnectToPipe(this.Name, this.Server);
        }

        public bool TryConnect()
        {
            DisposedCheck();
            bool ReturnVal = NamedPipeWrapper.TryConnectToPipe(this.Name, this.Server, out this.Handle);

            return ReturnVal;
        }
    }
}