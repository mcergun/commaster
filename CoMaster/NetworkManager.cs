using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CoMaster
{
    internal class NetworkManager
    {
        public static NetInterfaceInformation[] GetNetworkInformation()
        {
            var NetInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetInterfaceInformation[] ifcs = new NetInterfaceInformation[NetInterfaces.Length];
            for (int i = 0; i < NetInterfaces.Length; i++)
            {
                ifcs[i] = new NetInterfaceInformation();
                ReadIpProperties(NetInterfaces[i], ifcs[i]);
            }
            return ifcs;
        }

        public static void ReadIpProperties(NetworkInterface ifc, NetInterfaceInformation ifcprops)
        {
            ifcprops.IsUp = ifc.OperationalStatus == OperationalStatus.Up;
            ifcprops.Name = ifc.Name;
            ifcprops.Handle = ifc;
            ifcprops.Type = ifc.NetworkInterfaceType.ToString();
            ifcprops.Description = ifc.Description;
            var props = ifc.GetIPProperties();
            var phy = ifc.GetPhysicalAddress();
            ifcprops.IsDhcpEnabled = props.GetIPv4Properties().IsDhcpEnabled;
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

        public static void ApplySettings(NetInterfaceInformation ifc)
        {
            string commandStr1;
            string commandStr2;
            string execName = "netsh";
            if (ifc.IsDhcpEnabled || ifc.IpAddress == "0" || ifc.DhcpServerAddress == "0" || ifc.DnsServerAddress == "0" || ifc.GatewayAddress == "0")
            {
                commandStr1 = $"interface ip set address {ifc.Name} dhcp";
                commandStr2 = $"interface ip set dns {ifc.Name} dhcp";
            }
            else
            {
                commandStr1 = $"interface ip set address {ifc.Name} static {ifc.IpAddress} {ifc.SubnetAddress} {ifc.GatewayAddress} 1";
                commandStr2 = $"interface ip set dns {ifc.Name} static {ifc.DnsServerAddress}";
            }

            string cmd = $"/c {execName} {commandStr1} & {execName} {commandStr2}";
            ProcessStartInfo psi = new ProcessStartInfo("cmd", cmd);
            psi.Arguments = cmd;
            psi.Verb = "runas";
            psi.UseShellExecute = true;
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            Thread.Sleep(2000);
            p.WaitForExit();
        }
    }
}
