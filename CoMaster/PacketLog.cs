using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoMaster
{
    /// <summary>
    /// Used to log sent / received packets
    /// </summary>
    internal class PacketLog
    {
        public void Add(PacketLogItem pli)
        {
            lock(lockobj)
            {
                if (log.Count >= QUEUE_SIZE)
                {
                    log.Dequeue();
                }
                log.Enqueue(pli);
                OnItemLogged?.Invoke(this, null);
            }
        }
        public static PacketLog Instance { get { return instance; } }
        public Queue<PacketLogItem> List { get { return log; } }
        public event EventHandler OnItemLogged;

        private static PacketLog instance = new PacketLog();
        private static readonly int QUEUE_SIZE = 128;
        private PacketLog() { }
        private Queue<PacketLogItem> log = new Queue<PacketLogItem>(QUEUE_SIZE);
        object lockobj = new object();
    }
}
