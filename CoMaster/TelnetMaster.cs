using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Windows;
using System.Diagnostics;

namespace CoMaster
{
    // a.Parties may only request a change in option status; i.e., a
    // party may not send out a "request" merely to announce what mode it
    // is in.

    // b.If a party receives what appears to be a request to enter some
    // mode it is already in, the request should not be acknowledged.
    // This non-response is essential to prevent endless loops in the
    // negotiation.  It is required that a response be sent to requests
    // for a change of mode -- even if the mode is not changed.

    // c.Whenever one party sends an option command to a second party,
    // whether as a request or an acknowledgment, and use of the option
    // will have any effect on the processing of the data being sent from
    // the first party to the second, then the command must be inserted
    // in the data stream at the point where it is desired that it take
    // effect.  (It should be noted that some time will elapse between
    // the transmission of a request and the receipt of an
    // acknowledgment, which may be negative.  Thus, a host may wish to
    // buffer data, after requesting an option, until it learns whether
    // the request is accepted or rejected, in order to hide the
    // "uncertainty period" from the user.)

    internal class TelnetMaster
    {
        public TelnetMaster()
        {
        }

        public async void Call()
        {
            Pinger p = new Pinger("192.168.1.1", 1000);
            p.PacketTimedOut += P_PacketTimedOut;
            p.ReplyReceived += P_ReplyReceived;
            p.RequestFinished += P_RequestFinished;
            Task t = Task.Run(() => p.SendMultipleAsync("abcdefghijklmnopqrstuvwabcdefghi", 5));
            await t;
            //await p.SendMultipleAsync("abcdefghijklmnopqrstuvwabcdefghi", 6);
            //p.Destination = "google.com";

            //p.SendMultiple("abcdefghijklmnopqrstuvwabcdefghi", 3);
            //p.SendSingle("abcdefghijklmnopqrstuvwabcdefghi");
            //p.SendSingle("abcdefghijklmnopqrstuvwabcdefghi");
            //p.SendSingle("abcdefghijklmnopqrstuvwabcdefghi");

            System.Net.NetworkInformation.Ping p1 = new System.Net.NetworkInformation.Ping();
            var res = await p1.SendPingAsync("google.com", 1000, Encoding.ASCII.GetBytes("abcdefghijklmnopqrstuvwabcdefghi"));
            Debug.WriteLine("{0}, {1}", res.Status, res.RoundtripTime);
        }

        private void P_RequestFinished(object sender, PingResults e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[Total {0}]: Average ping-back time: {1}", e.TotalTries, e.TotalLatency / e.TotalTries);
            Debug.WriteLine(sb.ToString());
        }

        private void P_ReplyReceived(object sender, PingResults e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0} / {1}]: Received reply in {2} ms", e.LastTry, e.TotalTries, e.LastLatency);
            Debug.WriteLine(sb.ToString());
        }

        private void P_PacketTimedOut(object sender, PingResults e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("[{0} / {1}]: Timed out!", e.LastTry, e.TotalTries);
            Debug.WriteLine(sb.ToString());
        }
    }
}
