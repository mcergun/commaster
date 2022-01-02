using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace CoMaster
{
    public class PacketManager : IDisposable
    {
        public ProtocolType Protocol {
            get
            {
                return protocol;
            }
            set
            {
                if (protocol != value)
                {
                    if (value == ProtocolType.Tcp || value == ProtocolType.Udp)
                    {
                        protocol = value;
                        sourceChanged = true;
                        destinationChanged = true;
                    }
                }
            }
        }

        public int LocalPort
        {
            get { return localPort; }
            set
            {
                if (localPort != value)
                {
                    sourceChanged = true;
                    localPort = value;
                }
            }
        }

        public int RemotePort
        {
            get { return remotePort; }
            set
            {
                if (remotePort != value)
                {
                    remotePort = value;
                    destinationChanged = true;
                }
            }
        }

        public string RemoteAddress
        {
            get { return remoteAddress; }
            set
            {
                if (remoteAddress != value)
                {
                    remoteAddress = value;
                    destinationChanged = true;
                }    
            }
        }

        public bool IsConnected
        {
            get
            {
                if (protocol == ProtocolType.Tcp)
                {
                    return tcpClient != null && tcpClient.Client != null && tcpClient.Connected;
                }
                else
                {
                    return false;
                }
            }
        }

        public PacketManager()
        {
        }

        public void Connect(int srcPort, string hostAddress, int dstPort)
        {
            remoteHost = NetAddressHelper.ParseAddress(hostAddress, dstPort) as IPEndPoint;
            RemoteAddress = hostAddress;
            RemotePort = dstPort;
            LocalPort = srcPort;
            Connect();
        }

        public void Connect()
        {
            if (sourceChanged)
            {
                Setup();
            }
            if (destinationChanged && protocol == ProtocolType.Tcp)
            {
                if (tcpClient.Connected) throw new InvalidOperationException("TCP Client is already connected!");
                tcpClient.Connect(remoteHost);
            }
            destinationChanged = false;
        }

        public void Disconnect()
        {
            if (protocol == ProtocolType.Tcp && tcpClient?.Connected == true)
            {
                CleanOldClients();
            }
        }

        public void Send(int srcPort, string hostAddress, int dstPort, string message)
        {
            remoteHost = NetAddressHelper.ParseAddress(hostAddress, dstPort) as IPEndPoint;
            RemoteAddress = hostAddress;
            RemotePort = dstPort;
            LocalPort = srcPort;
            if (sourceChanged)
            {
                Setup();
            }
            Send(Encoding.ASCII.GetBytes(message));
            destinationChanged = false;
        }

        public void Send(byte[] messageBytes)
        {
            if (protocol == ProtocolType.Tcp)
            {
                if (!tcpClient.Connected) throw new InvalidOperationException("Tcp Client is not connected!");
                tcpClient.GetStream().Write(messageBytes, 0, messageBytes.Length);
            }
            else
            {
                udpClient.Send(messageBytes, messageBytes.Length, remoteHost);
            }
            PacketLogItem pli = new PacketLogItem()
            {
                Sender = null,
                Receiver = remoteHost,
                Data = messageBytes,
                Timestamp = DateTime.Now,
                Type = protocol
            };
            PacketLog.Instance.Add(pli);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    CleanOldClients();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~PacketManager()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void CleanOldClients()
        {
            if (tcpClient != null)
            {
                tcpClient.Close();
                tcpClient.Dispose();
                tcpClient = null;
            }
            if (udpClient != null)
            {
                udpClient.Close();
                udpClient.Dispose();
                udpClient = null;
            }
            sourceChanged = true;
            destinationChanged = true;
        }

        public void Setup()
        {
            IPEndPoint local = new IPEndPoint(0, localPort);
            CleanOldClients();
            if (protocol == ProtocolType.Tcp)
            {
                tcpClient = new TcpClient(local);
            }
            else
            {
                udpClient = new UdpClient(local);
            }
            sourceChanged = false;
        }

        private bool disposedValue;
        private UdpClient udpClient;
        private TcpClient tcpClient;
        private IPEndPoint remoteHost;
        private int localPort;
        private int remotePort;
        private string remoteAddress;
        private bool sourceChanged = true;
        private bool destinationChanged = true;
        private ProtocolType protocol = ProtocolType.Unknown;
    }
}
