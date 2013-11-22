using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Scada.MainVision
{
    /// <summary>
    /// Interaction logic for SamplerControlPanel.xaml
    /// </summary>
    public partial class SamplerControlPanel : UserControl
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll")]
        public extern static int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessageTimeout(IntPtr hWnd, int msg, int wParam, StringBuilder lParam, int flags, int timeout, out IntPtr pdwResult);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr PostMessage(IntPtr hWnd, int msg, int wParam, int lParam);



        [DllImport("kernel32.dll")]
        public extern static int GetLastError();

        private const int WM_GETTEXT = 0x000D;

        private const int WM_COMMAND = 0x0111;

        private const int BN_CLICKED = 0;

        private const int BufferLength = 1024;

        private const int StartButtonHandle = 0x3B;

        private const int StopButtonHandle = 0x3A;

        private const int StatusTextBox = 0xF;

        private IntPtr formHandle;

        private bool isOpen = false;

        public SamplerControlPanel(string deviceKey)
        {
            InitializeComponent();
            this.DeviceKey = deviceKey;
        }

        private void OnControl(object sender, RoutedEventArgs e)
        {
            if (this.isOpen)
            {
                IntPtr btnHwnd = GetDlgItem(this.formHandle, StopButtonHandle);
                int l = MakeLong(StartButtonHandle, BN_CLICKED);
                PostMessage(this.formHandle, WM_COMMAND, l, (int)btnHwnd);
            }
            else
            {
                IntPtr btnHwnd = GetDlgItem(this.formHandle, StartButtonHandle);
                int l = MakeLong(StartButtonHandle, BN_CLICKED);
                PostMessage(this.formHandle, WM_COMMAND, l, (int)btnHwnd);
            }

            
        }

        public string DeviceKey
        {
            get;
            set;
        }

        private int MakeLong(int l, int h)
        {
            // ((LONG)(((WORD)(a & 0xffff)) | ((DWORD)((WORD)(b & 0xffff))) << 16))
            return (int)(short)(l & 0xffff) | ((int)(short)(h & 0xffff)) << 16;
        }

        private void IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsStarted())
            {
                this.StartButton.IsEnabled = false;
                this.StopButton.IsEnabled = true;
            }
            else
            {
                this.StartButton.IsEnabled = true;
                this.StopButton.IsEnabled = false;
            }
        }

        private bool IsStarted()
        {
            string status = this.GetText(this.formHandle, StatusTextBox);
            return status == "2";
        }


        private string GetText(IntPtr hWnd, int nCtrlId)
        {
            IntPtr edit = GetDlgItem(this.formHandle, nCtrlId);
            StringBuilder sb = new StringBuilder();
            
            if (edit != IntPtr.Zero)
            {
                IntPtr result = IntPtr.Zero;
                SendMessageTimeout(edit, WM_GETTEXT, BufferLength, sb, 0, 1000, out result);
                return sb.ToString();
            }

            return "";
        }

        private void SamplerPanelLoaded(object sender, RoutedEventArgs e)
        {
            string path = Environment.CurrentDirectory;
            string file = string.Format("{0}\\devices\\{1}\\0.9\\HWND.r", path, this.DeviceKey);

            byte[] bytes = new byte[32];

            using (FileStream fs = File.Open(file, FileMode.Open))
            {
                int r = fs.Read(bytes, 0, 32);
                string line = Encoding.ASCII.GetString(bytes, 0, r);

                this.formHandle = (IntPtr)int.Parse(line);
            }
        }
    }
}
