using CoMaster.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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
            try
            {
                NetworkManager.ApplySettings(SelectedInterface);
                int oldIdx = SelectedIndex;
                UpdateInterfaces();
                SelectedIndex = oldIdx;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateInterfaces()
        {
            try
            {
                Interfaces = NetworkManager.GetNetworkInformation();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            //SelectedInterface = Interfaces[0];
        }

        private NetInterfaceInformation selectedInterface;
        private int selectedIndex;
        private NetInterfaceInformation[] interfaces;
        private RelayCommand<object> apply;
    }
}
