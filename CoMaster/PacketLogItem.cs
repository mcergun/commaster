using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoMaster
{
    internal class PacketLogItem
    {
        public EndPoint Sender { get; set; }
        public EndPoint Receiver { get; set; }
        public byte[] Data { get; set; }
        public DateTime Timestamp { get; set; }
        public ProtocolType Type { get; set; }
    }
}
