using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace CoMaster.Converters
{
    internal class ProtocolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = null;
            if (value is ComboBoxItem itm)
            {
                str = itm.Content.ToString();
            }
            else if (value is string)
            {
                str = (string)value;
            }
            switch (str)
            {
                case "TCP": return ProtocolType.Tcp;
                case "UDP": return ProtocolType.Udp;
                default: throw new FormatException("Unknown Protocol!");
            }
        }
    }
}
