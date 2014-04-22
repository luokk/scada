using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Client.Tcp
{
    class DataPacketBuilder
    {
        public DataPacketBuilder()
        {
        }

        public DataPacket GetDataPacket(string deviceKey, Dictionary<string, object> data, bool realTime = false)
        {
            if (data.Count == 0)
            {
                return null;
            }
            DataPacket dp = new DataPacket(deviceKey, realTime);
            dp.Settings = Settings.Instance;
            dp.St = Value.SysSend;
            string sno = Settings.Instance.Sno;
            string eno = Settings.Instance.GetEquipNumber(deviceKey);
            string timeStr = string.Empty;
            if (data.ContainsKey("time"))
            {
                timeStr = (string)data["time"];
            }
            string dataTime = DeviceTime.Convert(DateTime.Parse(timeStr));
            dp.SetContent(sno, eno, dataTime, data);
            dp.Build();
            return dp;
        }

        public DataPacket GetShelterPacket(string deviceKey, Dictionary<string, object> data, bool realTime = false)
        {
            if (data.Count == 0)
            {
                return null;
            }
            DataPacket dp = new DataPacket(deviceKey, realTime);
            dp.Cn = string.Format("{0}", (int)SentCommand.DoorState);
            dp.Settings = Settings.Instance;
            dp.St = Value.SysSend;
            string sno = Settings.Instance.Sno;
            string eno = Settings.Instance.GetEquipNumber(deviceKey);
            string timeStr = string.Empty;
            if (data.ContainsKey("time"))
            {
                timeStr = (string)data["time"];
            }
            string dataTime = DeviceTime.Convert(DateTime.Parse(timeStr));
            dp.SetContent(sno, eno, dataTime, data);
            dp.Build();
            return dp;
        }

        public DataPacket GetFlowDataPacket(string deviceKey, Dictionary<string, object> data, bool realTime = false)
        {
            if (data.Count == 0)
            {
                return null;
            }
            DataPacket dp = new DataPacket(deviceKey, realTime, true);
            // Adjust 'CN' !!
            if (realTime)
            {
                dp.Cn = string.Format("{0}", (int)SentCommand.FlowData);
            }
            else
            {
                dp.Cn = string.Format("{0}", (int)ReceivedCommand.GetFlowData);
            }

            dp.Settings = Settings.Instance;
            dp.St = Value.SysSend;
            string sno = Settings.Instance.Sno;
            string eno = Settings.Instance.GetEquipNumber(deviceKey);
            string timeStr = (string)data["time"];
            string dataTime = DeviceTime.Convert(DateTime.Parse(timeStr));
            dp.SetContent(sno, eno, dataTime, data);
            dp.Build();
            return dp;
        }

        public DataPacket GetRunStatusPacket(string qn, string[] devices, string[] status)
        {
            DataPacket dp = new DataPacket(SentCommand.RunStatus);
            dp.QN = qn;
            dp.Settings = Settings.Instance;
            dp.St = Value.SysSend;
            string sno = Settings.Instance.Sno;
            string dateTime = DeviceTime.Convert(DateTime.Now);
            dp.SetRunStatusContent(sno, dateTime, devices, status);
            dp.Build();
            return dp;
        }

        public DataPacket GetAuthPacket()
        {
            DataPacket dp = new DataPacket(SentCommand.Auth);
            dp.Settings = Settings.Instance;
            dp.St = Value.SysSend;
            dp.Build();
            return dp;
        }

        public DataPacket GetKeepAlivePacket()
        {
            DataPacket dp = new DataPacket(SentCommand.KeepAlive);
            dp.Settings = Settings.Instance;
            dp.St = Value.SysSend;
            dp.Build();
            return dp;
        }

        internal DataPacket GetReplyPacket(string qn)
        {
            DataPacket dp = new DataPacket(SentCommand.Reply);
            dp.Settings = Settings.Instance;
            dp.BuildReply(qn, 1);
            return dp;
        }

        internal DataPacket GetResultPacket(string qn, int result = 1)
        {
            DataPacket dp = new DataPacket(SentCommand.Result);
            dp.Settings = Settings.Instance;
            dp.BuildResult(qn, result);
            return dp;
        }

        internal DataPacket GetNotifyPacket(string qn)
        {
            DataPacket dp = new DataPacket(SentCommand.Notify);
            dp.Settings = Settings.Instance;
            dp.BuildNotify(qn);
            return dp;
        }

        internal List<DataPacket> GetDataPackets(string deviceKey, DateTime dateTime, string content, bool history = false, string qn = null)
        {
            List<DataPacket> rets = new List<DataPacket>();
            int from = 0;
            const int MaxContent = 920;
            int count = content.Length / MaxContent + 2;
            int index = 1;

            string sno = Settings.Instance.Sno;
            string eno = Settings.Instance.GetEquipNumber(deviceKey);

            string dataTime = DeviceTime.Convert(dateTime);

            // Header packet
            DataPacket dp = null;
            if (!history)
                dp = new DataPacket(SentCommand.Data);
            else
                dp = new DataPacket(SentCommand.HistoryData);
            // Set settings
            dp.Settings = Settings.Instance;

            dp.Splitted = true;
            dp.PacketCount = count;
            dp.PacketIndex = index;
            dp.St = Value.SysSend;
            Dictionary<string, object> data = new Dictionary<string, object>();
            List<Settings.DeviceCode> codes = Settings.Instance.GetCodes(deviceKey);
            string contentCode = codes[0].Code;
            data.Add(contentCode, string.Empty);
            data.Add("time", dateTime.ToString());
            dp.SetContent(sno, eno, dataTime, data);
            dp.Build();
            rets.Add(dp);

            string pqn = dp.QN;

            while (true)
            {
                index += 1;
                dp = null;

                if (!history)
                    dp = new DataPacket(SentCommand.Data);
                else
                    dp = new DataPacket(SentCommand.HistoryData);
                dp.Settings = Settings.Instance;
                dp.Splitted = true;
                dp.PacketCount = count;
                dp.PacketIndex = index;
                dp.St = Value.SysSend;

                string c = content.Substring(from, Math.Min(MaxContent, content.Length - from));
                dp.QN = pqn;
                dp.SetContent(sno, eno, dataTime, c);
                dp.Build();

                rets.Add(dp);

                from += c.Length;
                if (from >= content.Length)
                    break;
                
            }
            return rets;
        }

        internal DataPacket GetTimePacket(string qn)
        {
            DataPacket dp = new DataPacket(SentCommand.GetTime);
            dp.Settings = Settings.Instance;
            dp.QN = qn;
            dp.St = Value.SysSend;
            dp.BuildGetTime(DeviceTime.Convert(DateTime.Now));
            return dp;
        }

        internal DataPacket GetExceptionNotifyPacket(string deviceKey, bool p)
        {
            DataPacket dp = new DataPacket(p ? SentCommand.WentException : SentCommand.AfterException);
            dp.Settings = Settings.Instance;
            
            string sno = Settings.Instance.Sno;
            string dateTime = DeviceTime.Convert(DateTime.Now);
            dp.St = Value.SysSend;
            dp.SetExceptionNotifyContent(sno, dateTime, deviceKey, (p ? "0" : "1"));
            dp.Build();
            return dp;
        }

        internal DataPacket GetThresholdPacket(string polId, string eno, string v1, string v2)
        {
            DataPacket dp = new DataPacket(SentCommand.GetAlertThreshold);
            dp.Settings = Settings.Instance;

            string sno = Settings.Instance.Sno;
            string dateTime = DeviceTime.Convert(DateTime.Now);
            dp.St = Value.SysSend;
            dp.SetThresholdContent(sno, eno, polId, v1, v2);
            dp.Build();
            return dp;
        }

        internal DataPacket GetDoorStatePacket(string state)
        {
            DataPacket dp = new DataPacket(SentCommand.DoorState);
            dp.Settings = Settings.Instance;

            string sno = Settings.Instance.Sno;
            string dateTime = DeviceTime.Convert(DateTime.Now);
            dp.St = Value.SysSend;
            dp.SetDoorStateContent(sno, dateTime, state);
            dp.Build();
            return dp;
        }
    }
}
