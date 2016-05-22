using StreamDownloaderDownload;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.Download.JSON;
using StreamDownloaderDownload.Hosts;
using System.Net;
using System.Threading.Tasks;

namespace StreamDownloader
{
    public class DownloadContainer
    {
        private DownloadStatus _status;
        private readonly StreamDownloaderControls.UserControls.DownloadListItem _listItem;
        private readonly DownloadData _downloadData;
        private DownloadTask _downloadTask;

        public DownloadContainer(StreamDownloaderControls.UserControls.DownloadListItem listItem, DownloadData downloadData)
        {
            _listItem = listItem;
            _downloadData = downloadData;
        }

        public async Task Initialize()
        {
            if (_downloadData.Progress.Status == (int)DownloadStatus.CONTINUNING_LATER && !await Refetch(_downloadData.SourceURL))
                return;
            else
                _downloadData.FileHost = ((MainWindow)(((System.Windows.Controls.Grid)((System.Windows.Controls.ListBox)_listItem.Parent).Parent).Parent)).GetHost(_downloadData.RawURL);

            var linkFetch = new LinkFetchTask(_downloadData.FileHost, _downloadData.RawURL);
            linkFetch.Host.FetchStatusChanged += (sender, e) => { UpdateUIStatus(e.Message); };
            var fetchResponse = await linkFetch.Fetch();

            if (fetchResponse.Item2 == LinkFetchResult.FAILED)
            {
                _status = DownloadStatus.ERROR;
                return;
            }
            _downloadData.SourceURL = fetchResponse.Item1;
        }

        /// <summary>
        /// Finish preparation and start download.
        /// </summary>
        public void Start()
        {
            if (Utils.RoundDown((int)_status) == 30)
                return;
            _downloadTask = new DownloadTask(_downloadData, ((Properties.Settings.Default.CustomDownloadBufferSize) ? Properties.Settings.Default.DownloadBufferSize : Properties.Settings.Default.DEFAULT_DownloadBufferSize));
            _downloadTask.DownloadProgressChanged += _listItem.DownloadProgressChanged;
            _downloadTask.DownloadStatusChanged += _listItem.DownloadStatusChanged;
            _downloadTask.Start();
        }

        //STATUS
        public void UpdateUIStatus(string message)
        {
            _listItem.Status = message;
        }

        private async Task<bool> Refetch(string url)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
            request.ContentType = "application/x-www-form-urlencoded";

            try
            {
                using (var response = await request.GetResponseAsync())
                    if (((HttpWebResponse)response).StatusCode == HttpStatusCode.Gone)
                        return true;
            }
            catch
            {
                return true;
            }
            return false;
        }
    }
}