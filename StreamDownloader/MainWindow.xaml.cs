﻿using System;
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
        private HashSet<Task> _ActiveDownloads = new HashSet<Task>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void DownloadSubmit_Click(object sender, RoutedEventArgs e)
        {
            var cDownload = await CreateDownload.ShowDialog(((ContentControl)GetTemplateChild("MDI")), _Hosts, _FileExtensions);
            await ProcessDownloadAsync(cDownload);
        }

        private async Task ProcessDownloadAsync(CreateDownload cDownload)
        {
            var listItem = new DownloadListItem($"{cDownload.FileName}{cDownload.SelectedFileExtension.Extension}", cDownload.DownloadFolder, cDownload.DownloadLink, 0);
            listBox.Items.Add(listItem);

            LinkFetchResult response = LinkFetchResult.FAILED;

            var linkFetch = new LinkFetchTask((Host)Activator.CreateInstance(cDownload.SelectedHost.GetType()), cDownload.DownloadLink);
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
            RegisterHost("Vivo", typeof(Vivo));
            RegisterHost("StreamCloud", typeof(StreamCloud));
            RegisterHost("PowerWatch", typeof(StreamCloud));
            _FileExtensions.Add(new FileExtensionListItem("mp4", new MP4()));
            base.OnApplyTemplate();
        }

        protected void RegisterHost(string displayName, Type host)
        {
            _Hosts.Add(new HostListItem(displayName, host));
        }
    }
}