using System;

namespace Scada.Common
{
    public interface IChannelManager
    {
        void Initialize();

        void Stop();

        string HandleRequest(string request);

        bool Listen { get; set; }

        void WakeUp();

        void RemoveServerChannel(object param);
    }
}
