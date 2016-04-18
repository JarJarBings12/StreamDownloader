using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDownloaderControls;
using StreamDownloaderDownload;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.Hosts;

namespace StreamDownloader
{
    public class DownloadContainer
    {

        private DownloadStatus _status;
        private readonly DownloadListItem _listItem;
        private readonly Host _host;
        private readonly string _rawLink;
        private string _source;
        private readonly string _downloadFolder;
        private readonly string _fileName;
        private readonly string _fileExtension;
        private readonly uint _bufferSize;

        private DownloadTask _downloadTask;

        public DownloadContainer(DownloadListItem listItem, Host host, string rawLink, string downloadFolder, string fileName, string fileExtension)
        {
            _listItem = listItem;
            _host = host;
            _rawLink = rawLink;
            _source = "";
            _downloadFolder = downloadFolder;
            _fileName = fileName;
            _fileExtension = fileExtension;
            _bufferSize = Properties.Settings.Default.DownloadBufferSize;
        }

        public async Task Initialize()
        {
            var linkFetch = new LinkFetchTask((Host)Activator.CreateInstance(_host.GetType()), _rawLink);
            linkFetch.Host.FetchStatusChanged += (sender, e) => { UpdateUIStatus(e.Message); };
            var fetchResponse = await linkFetch.Fetch();
            
            if (fetchResponse.Item2 == LinkFetchResult.FAILED)
            {
                _status = DownloadStatus.ERROR;
                return;
            }
            _source = fetchResponse.Item1;
        }


        public void Start()
        {
            if (Utils.RoundDown((int)_status) == 30)
                return;
            _downloadTask = new DownloadTask(_downloadFolder, Properties.Settings.Default.TempDownloadFolder, _fileName, _fileExtension, _source, _rawLink,_bufferSize);
            _downloadTask.DownloadProgressChanged += _listItem.DownloadProgressChanged;
            _downloadTask.DownloadStatusChanged += _listItem.DownloadStatusChanged;
            _downloadTask.Start();
        }

        //STATUS
        public void UpdateUIStatus(string message)
        {
            _listItem.Status = message;
        }
    }
}
