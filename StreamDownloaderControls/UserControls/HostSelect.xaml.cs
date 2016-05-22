using StreamDownloaderDownload.Download.JSON;
using StreamDownloaderDownload.Hosts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamDownloaderControls.UserControls
{
    /// <summary>
    /// Interaction logic for DownloadItem2.xaml
    /// </summary>
    public partial class HostSelect: UserControl
    {
        #region C# Variables and properties

        public string DownloadURL
        {
            get { return (string)base.GetValue(DownlaodUrlProperty); }
            set { base.SetValue(DownlaodUrlProperty, value); }
        }

        public IEnumerable<HostListItem> SupportedHosts
        {
            get { return (IEnumerable<HostListItem>)base.GetValue(SupportedHostsProperty); }
            set { base.SetValue(SupportedHostsProperty, value); }
        }

        public HostListItem SelectedHost
        {
            get { return (HostListItem)base.GetValue(SelectedHostProperty); } //Create new instance of the type.
            set { base.SetValue(SelectedHostProperty, value); }
        }

        private DownloadData _downloadData
        {
            get
            {
                var dd = new DownloadData()
                {
                    FileHost = (base.GetValue(SelectedHostProperty) == null) ? null : (Host)Activator.CreateInstance(SelectedHost.FileHost),
                    RawURL = DownloadURL,
                    SourceURL = string.Empty,
                    DownloadDestination = string.Empty,
                    TempFileDestination = string.Empty,
                    FileName = string.Empty,
                    FullFileName = string.Empty,
                    FileExtension = string.Empty,
                    TempHostSettings = new Hashtable()
                };

                dd.TempHostSettings.Add("cancel", _cancel);

                return dd;
            }
        }

        private volatile bool _release = false;
        private volatile bool _cancel = false;

        #endregion C# Variables and properties

        #region WPF properties

        public static readonly DependencyProperty DownlaodUrlProperty = DependencyProperty.RegisterAttached("DownloadUrl", typeof(string), typeof(HostSelect), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty SupportedHostsProperty = DependencyProperty.RegisterAttached("SupportedHosts", typeof(IEnumerable), typeof(HostSelect), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty SelectedHostProperty = DependencyProperty.RegisterAttached("SelectedHost", typeof(HostListItem), typeof(HostSelect), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        #endregion WPF properties

        #region Constructors

        public HostSelect()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #endregion Constructors

        #region Public functions

        /// <summary>
        /// Open a new host select.
        /// </summary>
        /// <param name="showIn"></param>
        /// <param name="supportedHosts"></param>
        /// <returns></returns>
        public static async Task<DownloadData> OpenSelect(ListBox showIn, List<HostListItem> supportedHosts)
        {
            HostSelect temp = new HostSelect();
            temp.SupportedHosts = supportedHosts;
            showIn.Items.Insert(0, temp);
            await WaitToContinue(temp);
            return temp._downloadData;
        }

        #endregion Public functions

        #region Private functions

        private bool IsSupported(string link)
        {
            return SupportedHosts.ToList().Find(x => ((Host)Activator.CreateInstance(x.FileHost)).BaseUrlPattern.IsMatch(link)) != null;
        }

        private Host GetHost(string link)
        {
            foreach (HostListItem h in SupportedHosts)
            {
                Host host = ((Host)Activator.CreateInstance(h.FileHost));
                if (host.BaseUrlPattern.IsMatch(link))
                    return host;
            }
            return null;
        }

        private HostListItem GetHostListItem(Host host)
        {
            return SupportedHosts.ToList().Find(x => x.DisplayName == host.HostName);
        }

        #region GUI Interaction logic

        private void submit(object sender, RoutedEventArgs e)
        {
            _release = true;
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            _cancel = true;
            _release = true;
        }

        #region Mouse wheel scroll

        /// <summary>
        /// Handel the mouse wheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandelMouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            double offset = textBox.HorizontalOffset + (e.Delta / 12);
            offset = (offset < 0) ? 0 : offset;
            textBox.ScrollToHorizontalOffset(offset);
        }

        #endregion Mouse wheel scroll

        #endregion GUI Interaction logic

        #endregion Private functions

        #region Protected functions

        protected void ValidateDownloadLink(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(DownloadURL))
                return;
            if (IsSupported(DownloadURL))
            {
                lb_HostSelect.SelectedValue = GetHostListItem(GetHost(DownloadURL));
            }
            else
            {
            }
        }

        protected static async Task WaitToContinue(HostSelect selectDialog)
        {
            while (!selectDialog._release)
                await Task.Delay(100);
        }

        #endregion Protected functions
    }
}