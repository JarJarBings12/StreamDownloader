using System;
using System.Net;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDownloaderDownload.Download.JSON;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Timers;
using System.IO;
using StreamDownloaderDownload;

namespace StreamDownloaderDownload.Download
{
    public delegate void DownloadProgressChangedHandler(object sender, DownloadProgressChangedEventArgs e);
    public delegate void DownloadStatusChangedHandler(object sender, DownloadStatusChangedEventArgs e);
  

    public class DownloadTask
    {

        #region variables and properties
        private readonly string _downloadFolder;
        private readonly string _tempFolder;
        private readonly string _fileName;
        private readonly string _fileType;
        private readonly string _rawDownloadUrl;
        private readonly string _downloadUrl;
        private Timer _statusUpdateTimer;

        private readonly FileDownload _fileDownload;
        private DownloadStatus _status = DownloadStatus.NOT_INITIALIZED;

        public string DownloadFolder => _downloadFolder;
        public string FileName => _fileName;
        public string FileType => _fileType;
        public string RawDownloadUrl => _rawDownloadUrl;
        public string DownloadUrl => _downloadUrl;
        public FileDownload Download => _fileDownload;
        public bool IsPaused => _fileDownload.IsPaused;
        public DownloadStatus Status => _status;
        #endregion

        public event DownloadProgressChangedHandler DownloadProgressChanged;
        public event DownloadStatusChangedHandler DownloadStatusChanged;

        #region constructor
        public DownloadTask(string downloadFolder, string fileName, string fileType, string downloadUrl, string rawDownloadUrl) : this(downloadFolder, StreamDownloader.DownloadTempFolder, fileName, fileType, downloadUrl, rawDownloadUrl)
        { }

        public DownloadTask(string downloadFolder, string tempFolder, string fileName, string fileType, string downloadUrl, string rawDownloadUrl) : this(downloadFolder, tempFolder, fileName, fileType, downloadUrl, rawDownloadUrl, StreamDownloader.ChunkSize)
        { }

        public DownloadTask(string downloadFolder, string tempFolder, string fileName, string fileType, string downloadUrl, string rawDownloadUrl, uint bufferSize)
        {
            _downloadFolder = downloadFolder;
            _tempFolder = tempFolder;
            _fileName = fileName;
            _fileType = fileType;
            _downloadUrl = downloadUrl;
            _rawDownloadUrl = rawDownloadUrl;

            _fileDownload = new FileDownload(downloadUrl, $"{tempFolder}\\{Guid.NewGuid()}.dtemp", $"{downloadFolder}\\{fileName}{fileType}", bufferSize, this);
            InitializeUpdateTimer();
        }

        public DownloadTask(string downloadFolder, string tempFolder, string fileName, string fileType, string downloadUrl, string rawDownliadUrl, uint bufferSize, ulong writtenChunks, ulong contentLength)
        {
            _downloadFolder = downloadFolder;
            _tempFolder = tempFolder;
            _fileName = fileName;
            _fileType = fileType;
            _downloadUrl = downloadUrl;
            _rawDownloadUrl = rawDownliadUrl;

            _fileDownload = new FileDownload(downloadUrl, $"{tempFolder}\\{Guid.NewGuid()}.dtemp", $"{downloadFolder}\\{fileName}{fileType}", StreamDownloader.ChunkSize, writtenChunks, contentLength, this);
            InitializeUpdateTimer();
        }
        #endregion

        /// <summary>
        /// Starts the download.
        /// </summary>
        public void Start()
        {
            _fileDownload.Start();
        }

        /// <summary>
        /// Pausing the download.
        /// </summary>
        public void Pause()
        {
            _fileDownload.Pause();
        }

        public void Cancel()
        {
            _fileDownload.Cancel();
        }

        /// <summary>
        /// Stops the download and write the current status into a file.
        /// </summary>
        public void ContinuingLater()
        {
            _fileDownload.ContinuingLater();
        }

        public static DownloadTask LoadSavedDownload(string tempFile)
        {
            StoredDownload download = DeserializeDownloadState(tempFile);
            return new DownloadTask(download.DownloadFolder, download.TempFolder, download.FileName, download.FileType, download.SourceUrl, download.RawUrl, download.DownloadStatus.ChunkSize, download.DownloadStatus.WrittenChunks, download.DownloadStatus.ContentLength);
        }

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

        protected internal void UpdateDownloadProgress(double value, ulong writtenBytes)
        {
            if (DownloadProgressChanged == null)
                return;
            DownloadProgressChanged(this, new DownloadProgressChangedEventArgs(value, writtenBytes));
        }

        protected internal void StartDownload()
        {
            if ((int)_status != 21)
                UpdateDownloadStatus("INITIALIZE", DownloadStatus.INITIALIZE);
            _statusUpdateTimer.Start();
        }

        protected internal void ShutdownDownload()
        {
            _statusUpdateTimer.Stop();
        }

        protected static internal StoredDownload DeserializeDownloadState(string tempFile)
        {
            var deserialize = new DataContractSerializer(typeof(StoredDownload));
            return (StoredDownload)deserialize.ReadObject(new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }
    }

}
