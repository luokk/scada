using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scada.Declare;
using System.Timers;
using Scada.Config;
using System.Runtime.InteropServices;
using OPC.Data;
using System.Windows.Forms;
using OPC.Data.Interface;

namespace Scada.Device.Siemens
{
    public class OpcDevice : Scada.Declare.Device
    {
        public class HandleCode
        {
            public HandleCode(int handle, string code)
            {
                this.Handle = handle;
                this.Code = code;
            }

            public int Handle { get; set; }
            public string Code { get; set; }
        }

        private OpcServer server;

        private OpcGroup group;

        private bool started = false;

        private System.Windows.Forms.Timer timer = null;

        private string ipAddr;

        private OPCItemResult[] results;


        private const string TSAP = ":1001:1001,";

        private string tsap = TSAP;

        private string serverId;

        private string insertSQL;

        private DateTime beginTime;

        private DateTime endTime;

        private DateTime latestTime;

        private string tableName;

        public OpcDevice(DeviceEntry entry)
        {
            this.Initialize(entry);
        }

        private void Initialize(DeviceEntry entry)
        {
            this.Name = entry[DeviceEntry.Name].ToString();
            this.Id = entry[DeviceEntry.Identity].ToString();
            this.DeviceConfigPath = entry[DeviceEntry.Path].ToString();
            this.Version = entry[DeviceEntry.Version].ToString();

            this.ipAddr = entry["IPADDR"].ToString();

            this.tableName = (StringValue)entry[DeviceEntry.TableName];
            if (!string.IsNullOrEmpty(tableName))
            {
                string tableFields = (StringValue)entry[DeviceEntry.TableFields];
                this.insertSQL = this.MakeInsertSQL(this.tableName, tableFields);
            }
            
            IValue v = entry["TSAP"];
            if (v != null)
            {
                this.tsap = v.ToString();
            }

        }

        public override void Start(string address)
        {
            this.server = new OpcServer();
            this.started = true;
            this.serverId = address;
        }

        public override void Stop()
        {
            this.started = false;
        }

        public override void Send(byte[] action, DateTime time)
        {
            string cmd = Encoding.ASCII.GetString(action);
            if (this.started)
            {
                if (cmd.IndexOf("connect") == 0)
                {
                    int b = cmd.IndexOf("Sid=");
                    if (b > 0)
                    {
                        int length = cmd.IndexOf(";", b) - b - 4;
                        this.Sid = cmd.Substring(b + 4, length);
                    }
                    else
                    {
                        this.Sid = string.Format("SID-AUTO:{0}", DateTime.Now.ToString());
                    }
                    
                    this.Connect();
                }
                else if (cmd == "disconnect")
                {
                    this.Disconnect();
                }
            }
        }

        private void Connect()
        {
            try
            {
                this.server.Connect(this.serverId);

                RecordManager.DoSystemEventRecord(this, "Connected to CPU", RecordType.Event, true);

                this.group = this.server.AddGroup("Group1", true, 2000);

                this.OnConnect();

                this.Write(new HandleCode(13, "2"));

                this.timer = new System.Windows.Forms.Timer();
                this.timer.Interval = 3000;
                this.timer.Tick += this.OnDataTimer;
                this.timer.Start();
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(this, string.Format("Connect:{0}", e.Message), RecordType.Error);
            }

        }


        private void Write(params HandleCode[] hc)
        {
            int[] h = hc.Select((i) => i.Handle).ToArray();
            string[] c = hc.Select((i) => i.Code).ToArray();
            try
            {
                int[] errors;
                if (this.group.SyncWrite(h, c, out errors))
                {
                    RecordManager.DoSystemEventRecord(this, "Write Succeeded", RecordType.Event, true);
                }
                else
                {
                    RecordManager.DoSystemEventRecord(this, string.Format("Write Failed: {0} = {1}", string.Join(", ", h), string.Join(", ", c)), RecordType.Error, true);
                }
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(this, string.Format("Write Exception: {0} = {1}, {2}", string.Join(", ", h), string.Join(", ", c), e.Message), RecordType.Error, true);
            }
        }

        private void OnDataTimer(object sender, EventArgs e)
        {
            DateTime time;
            if (this.OnRightTime(out time))
            {
                if (this.beginTime == default(DateTime))
                {
                    this.beginTime = time;
                }

                int[] handles = this.results.Select((r) => r.HandleServer).ToArray();
                OPCItemState[] states;
                if (this.group.SyncRead(OPCDATASOURCE.OPC_DS_DEVICE, handles, out states))
                {
                    object[] values = states.Select((s) => s.DataValue).ToArray();
                    string valueLine = string.Join(", ", values);
                    RecordManager.DoSystemEventRecord(this, "Read: " + valueLine, RecordType.Event, true);

                    this.latestTime = time;
                    object[] data = new object[] { time, this.Sid, this.beginTime, this.endTime, values[3], values[4], values[5], values[6] };
                    DeviceData deviceData = new DeviceData(this, data);
                    deviceData.InsertIntoCommand = this.insertSQL;
                    RecordManager.DoDataRecord(deviceData);
                }
                else
                {
                    // 
                    RecordManager.DoSystemEventRecord(this, "Read Faild", RecordType.Event, true);
                }
            }

        }

        private bool OnRightTime(out DateTime time)
        {
            time = default(DateTime);
            return true;
        }

        private void Disconnect()
        {
            try
            {
                this.started = false;
                this.MarkEndTime();

                this.server.Disconnect();

                this.timer.Stop();
                this.timer.Dispose();

                this.timer = null;
            }
            catch (Exception e)
            {
                RecordManager.DoSystemEventRecord(this, string.Format("Disconnect:{0}", e.Message), RecordType.Error);
            }
        }

        private void MarkEndTime()
        {
            DeviceData deviceData = new DeviceData(this, new object[]{});
            deviceData.InsertIntoCommand = string.Format("update {0} set EndTime='{1}' where time='{2}'", this.tableName, this.latestTime, this.latestTime);
            RecordManager.DoDataRecord(deviceData);
        }

        public override bool OnReceiveData(byte[] line)
        {
            return true;
        }

        protected void OnConnect()
        {
            List<OPCItemDef> items = new List<OPCItemDef>();
            string[] codes =
            {
                "VW134,WORD",
                "VD114,REAL",
                "VW159,WORD",
                "VD34,REAL",
                "VD54,REAL",
                "VD74,REAL",
                "VB485,BYTE",
                "VW313,WORD",
                "VW890,WORD",
                "A0.1,BOOL",
                "A0.2,BOOL",
                "A1.0,BOOL",
                "VB490,BYTE",
                "E0.3,BOOL",
                "AEW4,WORD",
                "AEW6,WORD",
                "A0.6,BOOL"
            };
            int i = 0;
            foreach (var code in codes)
            {
                i++;
                OPCItemDef item = new OPCItemDef(this.Code(code), true, i, VarEnum.VT_EMPTY);
                items.Add(item);
            }

            if (this.group.AddItems(items.ToArray(), out this.results))
            {
                RecordManager.DoSystemEventRecord(this, string.Format("AddItems OK"), RecordType.Event, true);
                int[] data = this.results.Select((r) => r.HandleServer).ToArray();

                RecordManager.DoSystemEventRecord(this, string.Join(", ", data), RecordType.Event, true);

            }
            else
            {
                RecordManager.DoSystemEventRecord(this, string.Format("AddItems Failed"), RecordType.Error, true);

            }
        }

        private string MakeInsertSQL(string tableName, string tableFields)
        {
            string[] fields = tableFields.Split(',');
            string atList = string.Empty;
            for (int i = 0; i < fields.Length; ++i)
            {
                string at = string.Format("@{0}, ", i + 1);
                atList += at;
            }
            atList = atList.TrimEnd(',', ' ');
            string sql = string.Format("insert into {0}({1}) values({2})", tableName, tableFields, atList);
            return sql;
        }

        protected string Code(string code)
        {
            return string.Format("2:{0}{1}{2}", this.ipAddr, this.tsap, code);
        }

        public object Sid { get; set; }
    }


    public class MDSDevice : OpcDevice
    {
        public MDSDevice(DeviceEntry entry)
            :base(entry)
        {
        }

    }

    public class AISDevice : OpcDevice
    {
        public AISDevice(DeviceEntry entry)
            : base(entry)
        {
        }

    }

}
