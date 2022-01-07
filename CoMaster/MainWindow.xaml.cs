using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace CoMaster
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        PacketManager pm = new PacketManager();
        public MainWindow()
        {
            InitializeComponent();
            NetInterfaceInformation[] ifcs = NetworkManager.GetNetworkInformation();
            PacketLog.Instance.OnItemLogged += Instance_OnItemLogged;

            TelnetMaster tm = new TelnetMaster();
            tm.Call();
        }

        private void Instance_OnItemLogged(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                liLog.ItemsSource = null;
                liLog.ItemsSource = PacketLog.Instance.List;
            });
        }
    }
}
