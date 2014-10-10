using Scada.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;

namespace Scada.Declare
{
    public class MDSDevice:Device 
    {
         private bool isVirtual = false;

        private bool isOpen = false;

        private DeviceEntry entry = null;

        private Timer timer = null;

        private string SourcePath;//MDS原始的文件路径

        private string TargetPath;//MDS 拷贝出来的文件的路径

        //以下是解析MDS数据文件所需要的变量，开始

        private string Doserate;

        private string lat;

        private string lon;

        private string speed;

        private string height;

        private string map;

        private string doserateex;

        private string ifatificial;

        private FileStream MDS_FileStream;

        private StreamReader MDS_StreamRead;

        private StreamReader MDS_StreamRead2;

        private string MDS_str;

        private string MDS_str2;

        private string[] MDS_Array;

        private string[] MDS_NewArray;

        private MemoryStream MDS_MemoryStream;

        private int Array_Long;

        private string MDS_Time;

        private int FirstStrflag;

        private int SecongStrflag;

        private int flag=0;

        private long offset;

        private string MDS_SID;
        
       
        //以上是解析MDS数据文件所需要的变量，结束

       

        private string strActionInterval;

        private string insertIntoCommand;

        public MDSDevice(DeviceEntry entry)
        {
            this.entry = entry;
            this.Initialize(entry);
        }

        ~MDSDevice()
        {
        }

        // Initialize the device
        private void Initialize(DeviceEntry entry)
        {
            this.Name = entry[DeviceEntry.Name].ToString();
            this.DeviceConfigPath = entry[DeviceEntry.Path].ToString();
            this.Version = entry[DeviceEntry.Version].ToString();
            this.Id = entry[DeviceEntry.Identity].ToString();

            this.SourcePath  = (StringValue)entry["SourcePath"];
            this.TargetPath  =(StringValue )entry ["TargetPath"];//不太清楚读取出来的值是否满足路径格式？？c:\\xml？
            this.strActionInterval = (StringValue)entry[DeviceEntry.ActionInterval];

            string tableName = (StringValue)entry[DeviceEntry.TableName];
            string tableFields = (StringValue)entry[DeviceEntry.TableFields];
            this.InitializeMDSDeviceTable(tableName, tableFields, out this.insertIntoCommand);
        }

        private void InitializeMDSDeviceTable(string tableName, string tableFields, out string insertIntoCommand)
        {
            string[] fields = tableFields.Split(',');
            string atList = string.Empty;
            for (int i = 0; i < fields.Length; ++i)
            {
                string at = string.Format("@{0}, ", i + 1);
                atList += at;
            }
            atList = atList.TrimEnd(',', ' ');
            string cmd = string.Format("insert into {0}({1}) values({2})", tableName, tableFields, atList);
            insertIntoCommand = cmd;
        }

        public bool IsVirtual
        {
            get { return this.isVirtual; }
        }

        private bool IsOpen
        {
            get
            {
                return this.isOpen;
            }
        }

        /// <summary>
        /// 
        /// Ignore the address parameter
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private bool Connect(string address)
        {
            bool connected = true;

            this.timer = new Timer(new TimerCallback(TimerCallback), null, 1000, int.Parse(strActionInterval) * 1000);
            this.isOpen = true;

            return connected;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="o"></param>
        private void TimerCallback(object o)
        {
           //主要功能在此填写,解析MDS数据文件
            string source = SourcePath + "\\values.tmp";
            string target = TargetPath + "\\values.tmp";
            bool iswrite = true;
            if (File.Exists(source))//判断源文件是否存在
            {
                if (!Directory.Exists(TargetPath))
                {
                    Directory.CreateDirectory(TargetPath);
                }
                File.Copy(source, target, iswrite);

                MDS_FileStream = new FileStream(target, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                MDS_StreamRead = new StreamReader(MDS_FileStream, Encoding.Default);
                MDS_str = MDS_StreamRead.ReadToEnd();
                if (this.flag == 0)         //第一次读取的标志
                {
                    this.FirstStrflag = MDS_str.Length - 33;      //第一次读进来的所有字符串长度，需要减掉33（数据末尾的校验码）
                    MDS_MemoryStream = new MemoryStream(Encoding.GetEncoding("GB2312").GetBytes(MDS_str));//使用内存映射，增加IO效率
                    using (MDS_StreamRead2 = new StreamReader(MDS_MemoryStream))
                    {
                        MDS_str2 = MDS_StreamRead2.ReadToEnd();
                        MDS_Array = MDS_str2.Split(new Char[] { '~' });//第一次分割，分隔符为“~”，数组的第一位是空的
                        Array_Long = MDS_Array.Length;
                        for (int a = 1; a < Array_Long - 1; a++)//第一个数据和最后一个数据，不能要
                        {
                            MDS_NewArray = MDS_Array[a].Split(new Char[] { ';' });
                            if (a ==1)
                            {
                                this.MDS_SID = "SID:"+Convert.ToDateTime(MDS_NewArray[3]).ToLocalTime().ToString("yyyyMMddHHmmss"); 
                               // this.MDS_SID = Convert.ToDateTime(MDS_NewArray[3]).ToLocalTime().ToString("yyyyMMddHHmmss"); 
                            }
                            this.lat = MDS_NewArray[1];
                            this.lon = MDS_NewArray[2];
                            this.Doserate = MDS_NewArray[9];
                            this.speed = MDS_NewArray[6];
                            this.height = MDS_NewArray[7];
                            this.map = MDS_NewArray[8];
                            this.doserateex = MDS_NewArray[11];
                            this.ifatificial = MDS_NewArray[10];
                            this.MDS_Time = MDS_NewArray[3];
                            MDS_FileStream.Close();
                            MDS_StreamRead2.Close();
                            MDS_StreamRead.Close();
                            MDS_MemoryStream.Close();
                            Record("");//一定要调用record方法

                        }
                        
                        this.flag++;
                    }
                }
                else if (this.flag != 0)
                {
                    this.SecongStrflag = MDS_str.Length;
                    this.offset = this.FirstStrflag;
                    MDS_FileStream.Seek(this.offset, SeekOrigin.Begin);
                    MDS_StreamRead = new StreamReader(MDS_FileStream, Encoding.Default);
                    string MDS_str3 = MDS_StreamRead.ReadToEnd();
                    if (MDS_str3.Length > 33)
                    {
                        MDS_Array = MDS_str3.Split(new Char[] { '~' });//第一次分割，分隔符为“~”，数组的第一位是空的
                        Array_Long = MDS_Array.Length;
                        for (int a = 1; a < Array_Long - 1; a++)//第一个数据和最后一个数据，不能要
                        {
                            MDS_NewArray = MDS_Array[a].Split(new Char[] { ';' });
                            this.lat = MDS_NewArray[1];
                            this.lon = MDS_NewArray[2];
                            this.Doserate = MDS_NewArray[9];
                            this.speed = MDS_NewArray[6];
                            this.height = MDS_NewArray[7];
                            this.map = MDS_NewArray[8];
                            this.doserateex = MDS_NewArray[11];
                            this.ifatificial = MDS_NewArray[10];
                            this.MDS_Time = MDS_NewArray[3];
                            MDS_FileStream.Close();
                            MDS_StreamRead2.Close();
                            MDS_StreamRead.Close();
                            MDS_MemoryStream.Close();
                            Record("");//一定要调用record方法
                        }
                        this.FirstStrflag = this.SecongStrflag - 33;
                        this.flag++;
                    }
                    else
                    {
                        return;
                    }

                }
            }
            else
            {
                return;
            }
           









          //  Record("");//一定要调用record方法

        }

        public override void Start(string address)
        {
            this.Connect(address);
        }

        public override void Stop()
        {
            if (this.timer != null)
            {
                this.timer.Dispose();
                this.timer = null;
            }
            isOpen = false;
        }

        public override void Send(byte[] action, DateTime time)
        {
        }

        private void Record(string str)
        {
           // DateTime time = DateTime.Now;
            DateTime time = Convert.ToDateTime(MDS_Time ).ToLocalTime ();
            object[] data = new object[]{ time, Doserate  ,lat  ,lon  ,speed  ,height  ,map  ,doserateex  ,ifatificial ,MDS_SID };//修改此行，便是修改插入数据库的项

            DeviceData dd = new DeviceData(this, data);
            dd.InsertIntoCommand = this.insertIntoCommand;

            this.SynchronizationContext.Post(this.DataReceived, dd);
        }

        public override bool OnReceiveData(byte[] line)
        {
            return false;
        }
    }
    
}
