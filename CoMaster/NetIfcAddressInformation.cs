using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CoMaster
{
    internal struct NetIfcAddressInformation
    {
        public string Name { get; set; }
        public string Description  { get; set; }
        public bool IsUp  { get; set; }
        public string IpAddress  { get; set; }
        public string MacAddress  { get; set; }
        public string SubnetAddress  { get; set; }
        public string GatewayAddress  { get; set; }
        public string DhcpServerAddress  { get; set; }
        public string DnsServerAddress { get; set; }
    }

    internal class NetworkConfigurationReader
    {
        public static NetIfcAddressInformation[] GetNetworkInformation()
        {
            var NetInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetIfcAddressInformation[] ifcs = new NetIfcAddressInformation[NetInterfaces.Length];
            for (int i = 0; i < NetInterfaces.Length; i++)
            {
                ReadIpProperties(NetInterfaces[i], ref ifcs[i]);
            }
            return ifcs;
        }

        private static void ReadIpProperties(NetworkInterface ifc, ref NetIfcAddressInformation ifcprops)
        {
            ifcprops.IsUp = ifc.OperationalStatus == OperationalStatus.Up;
            ifcprops.Name = ifc.Name;
            ifcprops.Description = ifc.Description;
            var props = ifc.GetIPProperties();
            var phy = ifc.GetPhysicalAddress();
            if (!phy.Equals(PhysicalAddress.None))
            {
                ifcprops.MacAddress = string.Join(":", phy.GetAddressBytes().Select(b => b.ToString("X2")));
            }
            if (props != null)
            {
                var uniAddr = props.UnicastAddresses.
                    FirstOrDefault(a => a.Address.AddressFamily == AddressFamily.InterNetwork);
                if (uniAddr != null)
                {
                    ifcprops.IpAddress = uniAddr.Address.ToString();
                    ifcprops.SubnetAddress = uniAddr.IPv4Mask.ToString();
                }
                var gwAddr = props.GatewayAddresses.
                    FirstOrDefault(gw => gw.Address.AddressFamily == AddressFamily.InterNetwork);
                if (gwAddr != null) ifcprops.GatewayAddress = gwAddr.Address.ToString();
                ifcprops.DhcpServerAddress = props.DhcpServerAddresses.
                    FirstOrDefault(dhcp => dhcp.AddressFamily == AddressFamily.InterNetwork)?.ToString();
                ifcprops.DnsServerAddress = props.DnsAddresses.
                    FirstOrDefault(dns => dns.AddressFamily == AddressFamily.InterNetwork)?.ToString();
            }
        }
    }
}
