using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamDownloaderControls.UserControls
{
    /// <summary>
    /// Interaction logic for DownloadListItem.xaml
    /// </summary>
    public partial class HostListItem: UserControl
    {
        #region C# variables and properties

        public string DisplayName
        {
            get { return (string)base.GetValue(HostNameProperty); }
            set { base.SetValue(HostNameProperty, value); }
        }

        public BitmapImage HostIcon
        {
            get { return (BitmapImage)base.GetValue(HostIconProperty); }
            set { base.SetValue(HostIconProperty, value); }
        }

        public Type FileHost
        {
            get;
            set;
        }

        #endregion C# variables and properties

        #region WPF properties

        public static readonly DependencyProperty HostNameProperty = DependencyProperty.RegisterAttached("DisplayName", typeof(string), typeof(StreamDownloaderControls.UserControls.HostListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty HostIconProperty = DependencyProperty.RegisterAttached("HostIcon", typeof(BitmapImage), typeof(StreamDownloaderControls.UserControls.HostListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        #endregion WPF properties

        public HostListItem()
        {
            InitializeComponent();
            DisplayName = "N/A";
        }
    }
}