using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace StreamDownloaderControls
{
    public class HostListItem: Control
    {
        #region C# variables and properties
        public string DisplayName
        {
            get { return (string)base.GetValue(HosterNameProperty); }
            set { base.SetValue(HosterNameProperty, value); }
        }

        public Type Hoster { get; }

        #endregion

        #region WPF properties
        public static readonly DependencyProperty HosterNameProperty = DependencyProperty.RegisterAttached("Name", typeof(string), typeof(HostListItem));
        #endregion

        #region constructors
        static HostListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HostListItem), new FrameworkPropertyMetadata(typeof(HostListItem)));
        }

        public HostListItem(string displayName, Type hoster)
        {
            DisplayName = displayName;
            Hoster = hoster;
        }
        #endregion
    }
}
