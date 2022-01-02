using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoMaster
{
    internal class NetworkConstants
    {
        public static readonly int IP_HEADER_LEN = 20;
    }

    internal enum ICMPTypes : byte
    {
        EchoRequest = 0x08,
        EchoReply = 0x00
    }
}
