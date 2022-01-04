using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace CoMaster
{
    internal class NetIfcAddressInformation
    {
        public NetworkInterface Handle { get; set; }
        public string Name { get; set; }
        public string Description  { get; set; }
        public bool IsUp  { get; set; }
        public string IpAddress  { get; set; }
        public string MacAddress  { get; set; }
        public string SubnetAddress  { get; set; }
        public string GatewayAddress  { get; set; }
        public string DhcpServerAddress  { get; set; }
        public string DnsServerAddress { get; set; }

        public void ApplySettings()
        {
            // Currently only IP, Subnet and Gateway settings are monitored
            string commandStr1;
            string commandStr2;
            string execName = "netsh";
            if (IpAddress == "0" || DhcpServerAddress == "0" || DnsServerAddress == "0" || GatewayAddress == "0")
            {
                commandStr1 = $"interface ip set address {Name} dhcp";
                commandStr2 = $"interface ip set dns {Name} dhcp";
            }
            else
            {
                commandStr1 = $"interface ip set address {Name} static {IpAddress} {SubnetAddress} {GatewayAddress} 1";
                commandStr2 = $"interface ip set dns {Name} static {DnsServerAddress}";
            }

            string cmd = $"/c {execName} {commandStr1} & {execName} {commandStr2}";
            //Process p = new Process();
            //ProcessStartInfo psi = new ProcessStartInfo("netsh", commandStr1);
            //ProcessStartInfo psi2 = new ProcessStartInfo("netsh", commandStr2);
            //psi.Verb = "runas";
            //psi2.Verb = "runas";
            //psi.WindowStyle = ProcessWindowStyle.Hidden;
            //psi2.WindowStyle = ProcessWindowStyle.Hidden;
            //p.StartInfo = psi;
            //p.Start();
            //p.WaitForExit();
            //p.StartInfo = psi2;
            //p.Start();
            //p.WaitForExit();
            ProcessStartInfo psi = new ProcessStartInfo("cmd", cmd);
            psi.Arguments = cmd;
            psi.Verb = "runas";
            psi.UseShellExecute = true;
            Process p = new Process();
            p.StartInfo = psi;
            p.Start();
            Thread.Sleep(2000);
            p.WaitForExit();
            //ProcessStartInfo info = new ProcessStartInfo("cmd");
            //info.RedirectStandardInput = true;
            //info.Verb = "runas";
            //info.UseShellExecute = false;
            //Process p = new Process();
            //p.StartInfo = info;
            //p.Start();
            //using (StreamWriter sw = p.StandardInput)
            //{
            //    sw.WriteLine(commandStr1);
            //    Thread.Sleep(1000);
            //    sw.WriteLine(commandStr2);
            //    Thread.Sleep(1000);
            //}
        }
    }

    internal class NetworkConfigurationReader
    {
        public static NetIfcAddressInformation[] GetNetworkInformation()
        {
            var NetInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetIfcAddressInformation[] ifcs = new NetIfcAddressInformation[NetInterfaces.Length];
            for (int i = 0; i < NetInterfaces.Length; i++)
            {
                ifcs[i] = new NetIfcAddressInformation();
                ReadIpProperties(NetInterfaces[i], ifcs[i]);
            }
            return ifcs;
        }

        public static void ReadIpProperties(NetworkInterface ifc, NetIfcAddressInformation ifcprops)
        {
            ifcprops.IsUp = ifc.OperationalStatus == OperationalStatus.Up;
            ifcprops.Name = ifc.Name;
            ifcprops.Handle = ifc;
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
