using CoMaster.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace CoMaster.ViewModels
{
    internal class PacketSenderViewModel : Notifier
    {
        public PacketSenderViewModel()
        {
            Address = "192.168.1.1";
            Port = 50000;
            Message = "Hello There!";
            Protocol = ProtocolType.Udp;
            SourcePort = 0;
        }
        public string Address
        {
            get { return address; }
            set { address = value; NotifyPropertyChanged(); }
        }
        public int Port
        {
            get { return port; }
            set { port = value; NotifyPropertyChanged(); }
        }
        public int SourcePort
        {
            get { return sourcePort; }
            set { sourcePort = value; NotifyPropertyChanged(); }
        }
        public ProtocolType Protocol
        {
            get { return protocol; }
            set
            {
                protocol = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("ConnectString");
                CommandManager.InvalidateRequerySuggested();
            }
        }
        public string[] Protocols
        {
            get { return new string[] { "TCP", "UDP" }; }
        }
        public string Message
        {
            get { return message; }
            set { message = value; NotifyPropertyChanged(); }
        }
        public string ConnectString
        {
            get
            {
                if (Protocol == ProtocolType.Tcp)
                {
                    if (!packetManager.IsConnected) return "Connect";
                    else return "Disconnect";
                }
                else
                {
                    return "Connect";
                }
            }
        }
        public ICommand SendPacketCommand
        {
            get
            {
                if (sendPacket == null)
                {
                    sendPacket = new RelayCommand<object>(SendPacket, CanSendPacket);
                }
                return sendPacket;
            }
        }
        public ICommand ConnectCommand
        {
            get
            {
                if (connect == null)
                {
                    connect = new RelayCommand<object>(Connect, CanConnect);
                }
                return connect;
            }
        }

        public void SendPacket(object param)
        {
            try
            {
                packetManager.Protocol = Protocol;
                packetManager.Send(SourcePort, Address, Port, Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CanSendPacket(object param)
        {
            if (Protocol == ProtocolType.Tcp)
            {
                return canSend && packetManager.IsConnected;
            }
            else
            {
                return canSend;
            }
        }

        public async void Connect(object param)
        {
            try
            {
                packetManager.Protocol = Protocol;
                canConnect = false;
                await Task.Run(() =>
                {
                    if (!packetManager.IsConnected)
                    {
                        packetManager.Connect(SourcePort, Address, Port);
                    }
                    else
                    {
                        packetManager.Disconnect();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                NotifyPropertyChanged("ConnectString");
                canConnect = true;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool CanConnect(object param)
        {
            return canConnect && Protocol == ProtocolType.Tcp;
        }

        private ICommand sendPacket;
        private ICommand connect;
        private string address;
        private int port;
        private int sourcePort;
        private string message;
        private ProtocolType protocol;
        private bool canConnect = true;
        private bool canSend = true;
        private PacketManager packetManager = new PacketManager();
    }
}
