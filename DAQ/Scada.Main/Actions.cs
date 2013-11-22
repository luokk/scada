using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Scada.Main
{
	public static class Actions
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="milliseconds"></param>
		/// <param name="action"></param>
		public static void Delay(int milliseconds, Action action)
		{
			Timer timer = new Timer();
			timer.Interval = milliseconds;
			timer.Tick += (object sender, EventArgs e) =>
			{
				action.Invoke();
				timer.Stop();
				timer.Dispose();
			};
			timer.Start();
		}


	}
}
