using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Scada.DAQ.Tools
{
    public partial class MainForm : Form
    {
        private SerialPort serialPort = null;

        private int baudRate = 9600;

        private string com = "COM1";

        public MainForm()
        {
            InitializeComponent();
        }

        private void Open()
        {
            this.com = this.comboBox1.Text;
            this.serialPort = new SerialPort(com);
             
            this.serialPort.BaudRate = this.baudRate;

            this.serialPort.Parity = Parity.None;
            this.serialPort.StopBits = StopBits.One; //(StopBits)this.stopBits;    //StopBits 1
            this.serialPort.DataBits = 8;// this.dataBits;   // DataBits 8bit
            this.serialPort.ReadTimeout = 10000;// this.readTimeout;

            this.serialPort.RtsEnable = true;
            this.serialPort.NewLine = "/r/n";	//?
            this.serialPort.DataReceived += serialPort_DataReceived;

            this.serialPort.Open();


        }

        private byte[] GetBytes(string s)
        {
            if (this.checkBox1.Checked)
            {
                string[] bs = s.Split(' ');
                List<byte> a = new List<byte>();
                foreach (string b in bs)
                {
                    if (b.Length > 0)
                    {
                        byte bt = (byte)int.Parse(b, NumberStyles.AllowHexSpecifier);
                        a.Add(bt);
                    }
                }
                return a.ToArray<byte>();
            }
            else
            {
                return Encoding.ASCII.GetBytes(s);
            }
        }

        private void Close()
        {
            this.serialPort.Close();
        }

        private delegate void AddDataDelegate(string data);

        void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(200);
            int n = this.serialPort.BytesToRead;
            byte[] buffer = new byte[n];

            int r = this.serialPort.Read(buffer, 0, n);

            StringBuilder sb = new StringBuilder();
            foreach (byte b in buffer)
            {
                //Convert.ToString(b, 16);
                string s = string.Format("{0} ", Convert.ToString(b, 16));
                sb.Append(s);
            }

            this.listBox1.Invoke(new AddDataDelegate(this.AddData), sb.ToString());
        }

        private void AddData(string data)
        {
            MessageBox.Show(data);
            this.listBox1.Items.Add(data);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Open();
            // MessageBox.Show("O");
        }

        private void SendClick(object sender, EventArgs e)
        {

            string s = this.comboBox2.Text;

            byte[] bytes = this.GetBytes(s);
            this.serialPort.Write(bytes, 0, bytes.Length);
            // MessageBox.Show("Send");
        }

        private void StopClick(object sender, EventArgs e)
        {
            Close();
        }

        private void comboBox1_SelectedValueChanged(object sender, EventArgs e)
        {
            this.com = this.comboBox2.Text;
        }


    }
}
