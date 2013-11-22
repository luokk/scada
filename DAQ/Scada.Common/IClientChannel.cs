using System;
using System.IO;

namespace Scada.Common
{
    public interface IClientChannel : IDisposable
    {
        string HandleRequest(string request);
        
        string HandleRequest(Stream request);
        
        object HandleRequest(object request);
        
        IClientChannel Create();
    }
}