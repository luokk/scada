using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Common
{
    public interface IMessageTimer
    {
        void Start(Action action);

        void Close();
    }

    public abstract class MessageTimerCreator
    {
        public abstract IMessageTimer CreateTimer(int interval);
    }
}
