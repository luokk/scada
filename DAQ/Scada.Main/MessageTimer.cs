
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Scada.Common;

namespace Scada.Main
{
    public class WinFormTimer : IMessageTimer
    {
        private Timer timer;

        private int interval = 0;

        private Action action;

        public WinFormTimer(int interval)
        {
            this.interval = interval;
        }

        public void Start(Action action)
        {
            this.action = action;
            this.timer = new Timer();
            this.timer.Interval = this.interval * 1000;
            this.timer.Tick += timerTickHandler;
            this.timer.Start();
        }

        public void Close()
        {
            this.timer.Stop();
            this.timer.Dispose();
        }

        private void timerTickHandler(object sender, EventArgs e)
        {
            this.action();
        }
    }

    internal class WinFormTimerCreator : MessageTimerCreator
    {

        public override IMessageTimer CreateTimer(int interval)
        {
            return new WinFormTimer(interval);
        }
    }
}
