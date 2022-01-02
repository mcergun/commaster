using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoMaster.ViewModels
{
    internal class InterfaceDisplayViewModel : Notifier
    {
        public NetIfcAddressInformation SelectedInterface
        {
            get
            {
                return selectedInterface;
            }
            set
            {
                selectedInterface = value;
                NotifyPropertyChanged();
            }
        }
        public NetIfcAddressInformation[] Interfaces
        {
            get
            {
                return interfaces;
            }
            set
            {
                interfaces = value;
                NotifyPropertyChanged();
            }
        }

        public InterfaceDisplayViewModel()
        {
            Interfaces = NetworkConfigurationReader.GetNetworkInformation();
            SelectedInterface = Interfaces[0];
        }

        private NetIfcAddressInformation selectedInterface;
        private NetIfcAddressInformation[] interfaces;
    }
}
