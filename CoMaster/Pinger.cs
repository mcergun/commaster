using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Timer = System.Timers.Timer;
using System.Threading;
using System.Diagnostics;

namespace CoMaster
{
    internal struct PingResults
    {
        public bool IsFinished;
        public int TotalTries;
        public int LastTry;
        public double LastLatency;
        public double TotalLatency;

        public void Reset(int totalTries = 0)
        {
            IsFinished = false;
            TotalTries = totalTries;
            LastTry = 0;
            LastLatency = 0;
            TotalLatency = 0;
        }
    }

    internal class Pinger : IDisposable
    {
        public event EventHandler<PingResults> ReplyReceived;
        public event EventHandler<PingResults> PacketTimedOut;
        public event EventHandler<PingResults> RequestFinished;
        public int TimeoutDuration { get; set; }
        public string Destination
        {
            get { return m_destination; }
            set
            {
                RemoteEp = NetAddressHelper.ParseAddress(value, 0);
                if (RemoteEp == null)
                {
                    throw new EntryPointNotFoundException();
                }
                m_destination = value;
            }
        }
        public EndPoint RemoteEp { get; set; }
        public bool HasTimerElapsed { get; set; }
        public PingResults Results { get { return m_results; } }

        public Pinger(string dest, int timeout)
        {
            TimeoutDuration = timeout;
            echoPacket = new ICMPEchoPacket();
            Destination = dest;
            pingSocket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Icmp)
            {
                SendTimeout = TimeoutDuration,
                ReceiveTimeout = TimeoutDuration
            };
        }

        public void Dispose()
        {
            pingSocket?.Shutdown(SocketShutdown.Both);
            pingSocket?.Close();
            pingSocket?.Dispose();
        }

        public void SendMultiple(string msg, int count)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
            PreparePacket(msgBytes);
            Stopwatch sw = null;
            m_results.Reset(count);

            while (m_results.LastTry < m_results.TotalTries)
            {
                sw = Stopwatch.StartNew();
                SendPingReceiveReply();
                if (m_results.LastLatency < TimeoutDuration)
                {
                    Thread.Sleep((int)(TimeoutDuration - m_results.LastLatency));
                }
            }
            RequestFinished?.Invoke(this, m_results);
        }

        public async Task SendMultipleAsync(string msg, int count)
        {
            await Task.Run(() => SendMultiple(msg, count));
        }

        public void SendSingle(string msg)
        {
            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
            PreparePacket(msgBytes);
            m_results.Reset(1);
            SendPingReceiveReply();
            RequestFinished?.Invoke(this, m_results);
        }

        public async Task SendSingleAsync(string msg)
        {
            await Task.Run(() => SendSingle(msg));
        }

        private void SendPingReceiveReply()
        {
            Stopwatch sw = Stopwatch.StartNew();
            bool packetSent = SendRequest();
            if (packetSent)
            {
                ReceiveReply();
                m_results.LastTry++;
                if (m_hasReply)
                {
                    double elapsed = sw.ElapsedMilliseconds;
                    m_results.LastLatency = elapsed;
                    m_results.TotalLatency += elapsed;
                    ReplyReceived?.Invoke(this, m_results);
                }
                else
                {
                    m_results.LastLatency = TimeoutDuration;
                    m_results.TotalLatency += TimeoutDuration;
                    PacketTimedOut?.Invoke(this, m_results);
                }
            }
            sw.Stop();
        }

        private bool SendRequest()
        {
            int ret = 0;
            try
            {
                ret = pingSocket.SendTo(echoPacket.Buffer, RemoteEp);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock &&
                    se.SocketErrorCode != SocketError.TimedOut)
                {
                    // This is not EAGAIN, throw
                    throw;
                }
            }
            PacketLogItem pli = new PacketLogItem()
            {
                Sender = localHost,
                Receiver = RemoteEp,
                Data = echoPacket.Buffer,
                Timestamp = DateTime.Now,
                Type = ProtocolType.Icmp
            };
            PacketLog.Instance.Add(pli);
            return ret == echoPacket.Buffer.Length;
        }

        private bool ReceiveReply()
        {
            EndPoint sender = RemoteEp;
            int ret = 0;
            m_hasReply = false;
            try
            {
                ret = pingSocket.ReceiveFrom(receiveBuffer, ref sender);
            }
            catch (SocketException se)
            {
                if (se.SocketErrorCode != SocketError.WouldBlock &&
                    se.SocketErrorCode != SocketError.TimedOut)
                {
                    throw;
                }
            }
            if (ret > NetworkConstants.IP_HEADER_LEN)
            {
                byte firstB = receiveBuffer[NetworkConstants.IP_HEADER_LEN];
                if (sender == RemoteEp && firstB == (byte)ICMPTypes.EchoReply)
                {
                    ICMPEchoPacket rcvPkt = new ICMPEchoPacket(receiveBuffer,
                        NetworkConstants.IP_HEADER_LEN, ret - NetworkConstants.IP_HEADER_LEN);
                    if (ICMPEchoPacket.DoFieldsMatch(echoPacket, rcvPkt))
                    {
                        m_hasReply = true;
                    }
                }
            }
            return m_hasReply;
        }

        private void PreparePacket(byte[] msg)
        {
            echoPacket.Type = ICMPTypes.EchoRequest;
            echoPacket.Code = 0;
            echoPacket.Identifier = 1;
            echoPacket.SequenceNumber = 44;
            echoPacket.CustomData = msg;
            echoPacket.PrepareBuffer();
        }

        private bool m_hasReply = false;
        private string m_destination = string.Empty;
        private PingResults m_results = new PingResults();

        private Socket pingSocket;
        private ICMPEchoPacket echoPacket;
        private EndPoint localHost;
        private byte[] receiveBuffer = new byte[65507];
    }
}
