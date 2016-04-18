using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StreamDownloaderControls;
using StreamDownloaderDownload.FileExtensions;
using StreamDownloaderDownload.FileExtensions.Default;
using StreamDownloaderDownload.Hosts;
using StreamDownloaderDownload.Hosts.Default;
using StreamDownloaderDownload.Download;

namespace StreamDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: FlatWindow
    {
        private StreamDownloaderDownload.StreamDownloader _streamDownloader = new StreamDownloaderDownload.StreamDownloader();
        private List<HostListItem> _Hosts = new List<HostListItem>();
        private List<FileExtensionListItem> _FileExtensions = new List<FileExtensionListItem>();
        private readonly Brush _placeholderGray = new SolidColorBrush(Color.FromRgb(209, 209, 209));
        private readonly Brush _fontcolorBlack = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private HashSet<Task> _activeDownloads = new HashSet<Task>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DownloadSubmit_Click(object sender, RoutedEventArgs e)
        {
            //Show Create download Dialog
            var cDownload = await CreateDownload.ShowDialog(Properties.Settings.Default.DownloadFolder, ((ContentControl)GetTemplateChild("MDI")), _Hosts, _FileExtensions);

            var listItem = new DownloadListItem($"{cDownload.FileName}{cDownload.SelectedFileExtension.Extension}", cDownload.DownloadFolder, cDownload.DownloadLink, 0);
            listBox.Items.Add(listItem);

            var downloadContainer = new DownloadContainer(listItem, cDownload.SelectedHost, cDownload.DownloadLink, cDownload.DownloadFolder, cDownload.FileName, cDownload.SelectedFileExtension.Extension);

            await downloadContainer.Initialize();
            downloadContainer.Start();
        }

        public override void OnApplyTemplate()
        {
            ((Button)GetTemplateChild("DownloadButton")).Click += DownloadSubmit_Click;
            ((Button)GetTemplateChild("SettingsButton")).Click += (sender, e) => { new Settings().ShowDialog(); };
            RegisterHost("Vivo", typeof(Vivo));
            RegisterHost("StreamCloud", typeof(StreamCloud));
            RegisterHost("PowerWatch", typeof(PowerWatch));
            _FileExtensions.Add(new FileExtensionListItem("mp4", new MP4()));
            base.OnApplyTemplate();
        }

        public override void EndInit()
        {
            if (Properties.Settings.Default.FIRST_RUN)
                Settings.Initialize();
            base.EndInit();
        }

        protected void RegisterHost(string displayName, Type host)
        {
            _Hosts.Add(new HostListItem(displayName, host));
        }
    }
}