using StreamDownloaderDownload.Download.JSON;
using System;
using System.Timers;

namespace StreamDownloaderDownload.Download
{
    public delegate void DownloadProgressChangedHandler(object sender, DownloadProgressChangedEventArgs e);

    public delegate void DownloadStatusChangedHandler(object sender, DownloadStatusChangedEventArgs e);

    public class DownloadTask
    {
        #region variables and properties

        private readonly DownloadData _downloadData;
        private Timer _statusUpdateTimer;

        private readonly FileDownload _fileDownload;
        private DownloadStatus _status = DownloadStatus.NOT_INITIALIZED;

        public string DownloadFolder => _downloadData.DownloadDestination;
        public string DownloadTempFolder => _downloadData.TempFileDestination;
        public string Filename => _downloadData.FileName;
        public string FileExtension => _downloadData.FileExtension;
        public string RawDownloadURL => _downloadData.RawURL;
        public string SourceDownloadURL => _downloadData.SourceURL;
        public FileDownload Download => _fileDownload;
        public DownloadStatus Status => _status;
        public bool IsPaused => _fileDownload.IsPaused;

        #endregion variables and properties

        public event DownloadProgressChangedHandler DownloadProgressChanged;

        public event DownloadStatusChangedHandler DownloadStatusChanged;

        #region constructor

        public DownloadTask(DownloadData data, uint bufferSize)
        {
            _downloadData = data;
            _fileDownload = new FileDownload(SourceDownloadURL, (data.Progress.Status == 22) ? data.TempFileDestination : $"{DownloadTempFolder}\\{Guid.NewGuid()}.dtemp", (data.Progress.Status == 22) ? data.DownloadDestination : $"{DownloadFolder}\\{Filename}{FileExtension}", bufferSize, _downloadData.Progress.WrittenBytes, _downloadData.Progress.ContenLength, this);
            InitializeUpdateTimer();
        }

        #endregion constructor

        #region Public functions

        /// <summary>
        /// Start the download.
        /// </summary>
        public void Start()
        {
            if ((int)_status != 21)
                UpdateDownloadStatus("INITIALIZE", DownloadStatus.INITIALIZE);
            _statusUpdateTimer.Start();
            _fileDownload.Start();
        }

        /// <summary>
        /// Pausing the download.
        /// </summary>
        public void Pause()
        {
            _fileDownload.Pause();
        }

        /// <summary>
        /// Stops the download and write the current status into a file.
        /// </summary>
        public void ContinuingLater()
        {
            _fileDownload.ContinuingLater();
        }

        /// <summary>
        /// Cancel the download and delete the downloaded file.
        /// </summary>
        public void Cancel()
        {
            _fileDownload.Cancel();
        }

        #endregion Public functions

        #region Private functions

        protected internal void SerializeDownloadState(string outputFile)
        {
            _downloadData.Progress.Status = (int)Status;
            _downloadData.Progress.ContenLength = _fileDownload.ContentLength.Value;
            _downloadData.Progress.WrittenBytes = _fileDownload.WrittenBytes;
            _downloadData.TempFileDestination = _fileDownload.TempFile;
            _downloadData.DownloadDestination = _fileDownload.FileDestination;
            _downloadData.SerializeDownloadState(outputFile);
        }

        #endregion Private functions

        #region Protected functions

        /// <summary>
        /// Sets a timer for the <seealso cref="DownloadProgressChanged"/> event, with a interval of 500 milliseconds.
        /// This should prevent that the UI thread freeze when to many downloads run at the same time.
        /// </summary>
        protected void InitializeUpdateTimer()
        {
            _statusUpdateTimer = new Timer(500);
            _statusUpdateTimer.Elapsed += (sender, e) => { UpdateDownloadProgress(((_fileDownload.WrittenBytes * 100) / _fileDownload.ContentLength.Value), _fileDownload.WrittenBytes); };
            _statusUpdateTimer.AutoReset = true;
        }

        /// <summary>
        /// Stops the update timer.
        /// </summary>
        protected internal void ShutdownUpdateTimer()
        {
            _statusUpdateTimer.Stop();
        }

        /// <summary>
        /// Update download status message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="status"></param>
        protected internal void UpdateDownloadStatus(string message, DownloadStatus status)
        {
            if (DownloadStatusChanged == null)
                return;

            if (Utils.RoundDown((int)_status) == 0 && Utils.RoundDown((int)status) != 10)
                throw new InvalidOperationException("Please initialize the download first!");

            if (Utils.RoundDown((int)_status) > Utils.RoundDown((int)status))
                throw new InvalidOperationException($"You can't go from {_status.ToString()} to {status.ToString()}");

            DownloadStatusChanged(this, new DownloadStatusChangedEventArgs(message, status, _fileDownload));
            _status = status;
        }

        /// <summary>
        /// Update download progress.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="writtenBytes"></param>
        protected internal void UpdateDownloadProgress(double value, ulong writtenBytes)
        {
            if (DownloadProgressChanged == null)
                return;
            DownloadProgressChanged(this, new DownloadProgressChangedEventArgs(value, writtenBytes));
        }

        #endregion Protected functions
    }
}