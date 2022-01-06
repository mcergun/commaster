using CoMaster.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoMaster.ViewModels
{
    internal class InterfaceDisplayViewModel : Notifier
    {
        public NetInterfaceInformation SelectedInterface
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
        public NetInterfaceInformation[] Interfaces
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
        public int SelectedIndex
        {
            get
            {
                return selectedIndex;
            }
            set
            {
                selectedIndex = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand<object> ApplyConfigurationCommand
        {
            get
            {
                if (apply == null)
                {
                    apply = new RelayCommand<object>(ApplyConfiguration);
                }
                return apply;
            }
        }

        public InterfaceDisplayViewModel()
        {
            UpdateInterfaces();
        }

        private void ApplyConfiguration(object obj)
        {
            NetworkManager.ApplySettings(SelectedInterface);
            int oldIdx = SelectedIndex;
            UpdateInterfaces();
            SelectedIndex = oldIdx;
        }

        private void UpdateInterfaces()
        {
            Interfaces = NetworkManager.GetNetworkInformation();
            //SelectedInterface = Interfaces[0];
        }

        private NetInterfaceInformation selectedInterface;
        private int selectedIndex;
        private NetInterfaceInformation[] interfaces;
        private RelayCommand<object> apply;
    }
}
