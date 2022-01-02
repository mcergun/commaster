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
        NetworkInterface[] NetInterfaces;
        IPInterfaceProperties[] IfcProperties;
        PacketManager pm = new PacketManager();
        public MainWindow()
        {
            InitializeComponent();
            NetIfcAddressInformation[] ifcs = NetworkConfigurationReader.GetNetworkInformation();
            PacketLog.Instance.OnItemLogged += Instance_OnItemLogged;

            NetInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            IfcProperties = new IPInterfaceProperties[NetInterfaces.Length];
            int i = 0;
            foreach (var ifc in NetInterfaces)
            {
                Debug.WriteLine("[{0}] {1}: {2}, {3}",
                    ifc.Name, ifc.Description, ifc.Speed, ifc.OperationalStatus);
                IfcProperties[i++] = ifc.GetIPProperties();
            }

            listInterfaces.AutoGeneratingColumn += ListInterfaces_AutoGeneratingColumn;
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

        private void ListInterfaces_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyName == "Id")
                e.Cancel = true;

            switch (e.PropertyName)
            {
                case "NetworkInterfaceType":
                    e.Column.Header = "Interface Type";
                    break;
                case "IsUp":
                    e.Column.Header = "State";
                    break;
                case "Name":
                case "Description":
                    e.Column.Header = e.PropertyName;
                    break;
                case "Id":
                case "IsReceiveOnly":
                case "SupportsMulticast":
                case "Speed":
                default:
                    e.Cancel = true;
                    break;
            }
        }
    }
}
