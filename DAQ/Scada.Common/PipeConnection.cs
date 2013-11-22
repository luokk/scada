using System;
using System.IO;

namespace Scada.Common
{
    public abstract class PipeConnection : IInterProcessConnection
    {
        protected PipeHandle Handle = new PipeHandle();

        protected string Name;

        protected bool disposed = false;

        protected int maxReadBytes;

        public string Read()
        {
            DisposedCheck();
            return NamedPipeWrapper.Read(Handle, maxReadBytes);
        }

        public byte[] ReadBytes()
        {
            DisposedCheck();
            return NamedPipeWrapper.ReadBytes(Handle, maxReadBytes);
        }

        public void Write(string text)
        {
            DisposedCheck();
            NamedPipeWrapper.Write(Handle, text);
        }

        public void WriteBytes(byte[] bytes)
        {
            DisposedCheck();
            NamedPipeWrapper.WriteBytes(Handle, bytes);
        }

        public abstract void Close();

        public abstract void Connect();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                NamedPipeWrapper.Close(this.Handle);
            }
            disposed = true;
        }

        internal void DisposedCheck()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException("The Pipe Connection is disposed.");
            }
        }

        public InterProcessConnectionState GetState()
        {
            DisposedCheck();
            return this.Handle.State;
        }

        public int NativeHandle
        {
            get
            {
                DisposedCheck();
                return (int)this.Handle.Handle;
            }
        }
    }
}