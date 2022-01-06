using System.Net.NetworkInformation;

namespace CoMaster
{
    internal class NetInterfaceInformation
    {
        public NetworkInterface Handle { get; set; }
        public string Name { get; set; }
        public string Description  { get; set; }
        public bool IsUp  { get; set; }
        public string Type { get; set; }
        public bool IsDhcpEnabled { get; set; }
        public string IpAddress  { get; set; }
        public string MacAddress  { get; set; }
        public string SubnetAddress  { get; set; }
        public string GatewayAddress  { get; set; }
        public string DhcpServerAddress  { get; set; }
        public string DnsServerAddress { get; set; }
    }
}
