using System;

namespace Scada.Common
{
    public interface IInterProcessConnection : IDisposable
    {
        
        int NativeHandle { get; }
        
        void Connect();
        
        void Close();
        
        string Read();
        
        byte[] ReadBytes();
        
        void Write(string text);
        
        void WriteBytes(byte[] bytes);
        
        InterProcessConnectionState GetState();
    }
}