using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Windows;

namespace CoMaster
{

    internal class ICMPEchoPacket
    {
        public ICMPTypes Type { get; set; }
        public byte Code { get; set; }
        public ushort Checksum { get; set; }
        public ushort Identifier { get; set; }
        public ushort SequenceNumber { get; set; }
        public byte[] CustomData { get; set; }
        public byte[] Buffer
        {
            get { return buf; }
        }

        public static bool DoFieldsMatch(ICMPEchoPacket pkt1, ICMPEchoPacket pkt2)
        {
            bool ret = (pkt1.Code == pkt2.Code &&
                pkt1.Identifier == pkt2.Identifier &&
                pkt1.SequenceNumber == pkt2.SequenceNumber &&
                pkt1.CustomData.Length == pkt2.CustomData.Length);
            for (int i = 0; ret && i < pkt1.CustomData.Length; i++)
            {
                ret &= pkt1.CustomData[i] == pkt2.CustomData[i];
            }
            return ret;
        }

        public ICMPEchoPacket()
        {
            CustomData = null;
        }

        public ICMPEchoPacket(byte[] newBuf, int offset = 0, int length = 0)
        {
            length = length == 0 ? newBuf.Length : length;
            if (newBuf[offset] == (byte)ICMPTypes.EchoReply ||
                newBuf[offset] == (byte)ICMPTypes.EchoRequest)
            {
                // This is an echo packet
                CalculateChecksum(newBuf, offset, length);
                if (Checksum == 0)
                {
                    // Checksum matches, parse rest of the packet
                    Type = (ICMPTypes)newBuf[offset];
                    Code = newBuf[offset + 1];
                    Identifier = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(newBuf, offset + 4));
                    SequenceNumber = (ushort)IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(newBuf, offset + 6));
                    if (length > 8)
                    {
                        CustomData = new byte[length - 8];
                        Array.Copy(newBuf, offset + 8, CustomData, 0, CustomData.Length);
                    }
                }
                else
                {
                    throw new FormatException("ICMP checksum doesn't match");
                }
            }
            else
            {
                throw new FormatException("Not a valid Echo Reply packet");
            }
        }

        public void PrepareBuffer()
        {
            int custLen = CustomData == null ? 0 : CustomData.Length;
            int totalLen = custLen + 2 * 3 + 2;
            // Align buffer to 2*n bytes
            buf = Enumerable.Repeat((byte)0, (totalLen + 1) / 2 * 2).ToArray();
            buf[0] = ((byte)Type);
            buf[1] = Code;
            // Checksum should be zero
            buf[2] = 0x00;
            buf[3] = 0x00;
            byte[] nid = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)Identifier));
            buf[4] = nid[0];
            buf[5] = nid[1];
            byte[] nsn = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)SequenceNumber));
            buf[6] = nsn[0];
            buf[7] = nsn[1];
            if (CustomData != null)
            {
                Array.Copy(CustomData, 0, buf, 8, custLen);
            }
            CalculateChecksum(buf);
            // Checksum is already calculated on network order bytes, no need for another change of orders
            byte[] ncs = BitConverter.GetBytes(Checksum);
            buf[2] = ncs[0];
            buf[3] = ncs[1];
        }

        void CalculateChecksum(byte[] pkt, int offset = 0, int length = 0)
        {
            // Temp variable for storing carries
            uint csum = 0;
            length = length == 0 ? pkt.Length : length;
            for (int i = 0; i < length; i += 2)
            {
                csum += BitConverter.ToUInt16(pkt, i + offset);
            }
            Checksum = (ushort)(~((csum >> 16) + (csum & 0xFFFF)));

        }

        private byte[] buf = null;
    }
}
