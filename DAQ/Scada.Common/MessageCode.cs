using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Scada.Common
{
	public static class Defines
	{
        [DllImport("User32.dll", EntryPoint = "PostMessage")]
        public static extern bool PostMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        public static extern bool SendMessage(IntPtr hWnd, int Msg, int wParam, ref CopyData lParam);

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);


        public const string LocalPipeName = "ScadaPipe";

        public const int WM_COPYDATA = 0x004A;


		public const int WM_KEEPALIVE = 0x006A;


		public const int KeepAlive = 232;


		// public const int KeepAliveInterval = 5000;


        public const int RescueCheckTimer = 5000;

	}
}
