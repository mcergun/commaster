using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoMaster
{
    internal class NetAddressHelper
    {
        public static EndPoint ParseAddress(string address, int port)
        {
            EndPoint ep;
            if (IPAddress.TryParse(address, out IPAddress ip))
            {
                ep = new IPEndPoint(ip, port);
            }
            else
            {
                ep = GetFirstEndpoint(address, port);
            }
            return ep;
        }

        public static EndPoint GetFirstEndpoint(string address, int port)
        {
            var addressList = Dns.GetHostEntry(address).AddressList;
            return new IPEndPoint(addressList.Where(a => a.AddressFamily == AddressFamily.InterNetwork).First(), port);
        }
    }
}
