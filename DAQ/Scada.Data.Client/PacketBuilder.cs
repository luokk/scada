using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scada.Data.Client
{
    public class PacketBuilder
    {
        internal List<Packet> GetPackets(string deviceKey, DateTime time, string content)
        {
            throw new NotImplementedException();
        }

        internal Packet GetPacket(string deviceKey, Dictionary<string, object> data, bool p)
        {
            Packet packet = new Packet("");
            packet.AddData(deviceKey, data);
            return packet;
        }

        internal Packet GetReplyCommandPacket()
        {
            Packet packet = new Packet("");
            return packet;
        }

        internal Packet GetFilePacket(string fileName)
        {
            Packet packet = new Packet("");
            packet.Path = fileName;
            return packet;
        }

        internal Packet CombinePacket(Packet packet1, Packet packet2)
        {
            if (packet1 != null)
            {
                packet1.AppendEntry(packet2.GetEntry());
                return packet1;
            }
            return packet2;
        }
    }
}
