using System;
using System.Collections.Generic;
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
using StreamDownloaderControls;
using StreamDownloaderDownload;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.Hosters;
using StreamDownloaderDownload.Hosters.Default;
using StreamDownloaderDownload.FileExtensions.Default;
using System.Threading;
using System.IO;
using System.ComponentModel;

namespace StreamDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: FlatWindow
    {
        private StreamDownloaderDownload.StreamDownloader _streamDownloader = new StreamDownloaderDownload.StreamDownloader();
        private readonly Brush _placeholderGray = new SolidColorBrush(Color.FromRgb(209, 209, 209));
        private readonly Brush _fontcolorBlack = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private HashSet<Task> _ActiveDownloads = new HashSet<Task>();

        public MainWindow()
        {
            InitializeComponent();
            _streamDownloader.RegisterHost(new Vivo());
            _streamDownloader.RegisterHost(new StreamCloud());
            _streamDownloader.RegisterHost(new PowerWatch());
            _streamDownloader.RegisterFileExtension(new MP4());
        }

        private async void DownloadSubmit_Click(object sender, RoutedEventArgs e)
        {
            var cDownload = await CreateDownload.ShowDialog(((ContentControl)GetTemplateChild("MDI")), _streamDownloader.SupportedHosters.ToList(), _streamDownloader.SupportedFilExtensions.ToList());
            await ProcessDownloadAsync(cDownload);
        }

        private async Task ProcessDownloadAsync(CreateDownload cDownload)
        {
            var listItem = new DownloadListItem(cDownload.FileName, cDownload.DownloadFolder, cDownload.DownloadLink, 0);
            listBox.Items.Add(listItem);

            LinkFetchResult response = LinkFetchResult.FAILED;

            var linkFetch = new LinkFetchTask((Hoster)Activator.CreateInstance(cDownload.SelectedHoster.GetType()), cDownload.DownloadLink);
            linkFetch.Host.FetchStatusChanged += (_sender, _e) => { listItem.Status = ((LinkFetchStatusChangedEventArgs)_e).Message; };
            linkFetch.Host.LinkFetchFinished += (_sender, _e) => { response = ((LinkFetchFinishedEventArgs)_e).Result; };

            var downloadLink = await linkFetch.Fetch();

            if (response == LinkFetchResult.FAILED)
                return;

            var downloadTast = StreamDownloaderDownload.StreamDownloader.CreateDownload(cDownload.DownloadFolder, StreamDownloaderDownload.StreamDownloader.DownloadTempFolder + @"\\", cDownload.FileName, cDownload.SelectedFileExtension.Extension, downloadLink, StreamDownloaderDownload.StreamDownloader.ChunkSize);
            downloadTast.DownloadProgressChanged += listItem.DownloadProgressChanged;
            downloadTast.DownloadStatusChanged += listItem.DownloadStatusChanged;
            downloadTast.DownloadBegin += (_sender, _e) => { listItem.CalculateFullContentLengths(downloadTast.Download.ContentLength.Value); };
            downloadTast.Start();
        }

        public override void OnApplyTemplate()
        {
            ((Button)GetTemplateChild("DownloadButton")).Click += DownloadSubmit_Click;
            base.OnApplyTemplate();
        }
    }
}