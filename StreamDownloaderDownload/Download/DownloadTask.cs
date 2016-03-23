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
using System.IO;

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
        private readonly string _downloadUrl;

        private readonly FileDownload _fileDownload;

        public string DownloadFolder => _downloadFolder;
        public string FileName => _fileName;
        public string FileType => _fileType;
        public string DownloadUrl => _downloadUrl;
        public bool IsPaused => _fileDownload.IsPaused;
        #endregion

        public event DownloadProgressChangedHandler DownloadProgressChanged;
        public event DownloadStatusChangedHandler DownloadStatusChanged;

        #region constructor

        public DownloadTask(string downloadFolder, string fileName, string fileType, string downloadUrl) : this(downloadFolder, StreamDownloader.DownloadTempFolder, fileName, fileType, downloadUrl)
        { }

        public DownloadTask(string downloadFolder, string tempFolder, string fileName, string fileType, string downloadUrl)
        {
            _downloadFolder = downloadFolder;
            _tempFolder = tempFolder;
            _fileName = fileName;
            _fileType = fileType;
            _downloadUrl = downloadUrl;

            _fileDownload = new FileDownload(downloadUrl, $"{tempFolder}{Guid.NewGuid()}.dtemp", $"{downloadFolder}{fileName}.{fileType}", StreamDownloader.ChunkSize, this);
        }

        public DownloadTask(string downloadFolder, string tempFolder, string fileName, string fileType, string downloadUrl, uint chunkSize)
        {
            _downloadFolder = downloadFolder;
            _tempFolder = tempFolder;
            _fileName = fileName;
            _fileType = fileType;
            _downloadUrl = downloadUrl;

            _fileDownload = new FileDownload(downloadUrl, $"{tempFolder}{Guid.NewGuid()}.dtemp", $"{downloadFolder}{fileName}.{fileType}", chunkSize, this);
        }

        public DownloadTask(string downloadFolder, string tempFolder, string fileName, string fileType, string downloadUrl, uint chunkSize, ulong writtenChunks, ulong contentLength)
        {

            _downloadFolder = downloadFolder;
            _tempFolder = tempFolder;
            _fileName = fileName;
            _fileType = fileType;
            _downloadUrl = downloadUrl;

            _fileDownload = new FileDownload(downloadUrl, $"{tempFolder}{Guid.NewGuid()}.dtemp", $"{downloadFolder}{fileName}.{fileType}", StreamDownloader.ChunkSize, writtenChunks, contentLength, this);
        }

        #endregion

        public void Start()
        {
            _fileDownload.Start();
        }

        public void Pause()
        {
            _fileDownload.Pause();
        }

        public void ContinuingLater()
        {
            _fileDownload.ContinuingLater();
        }

        protected internal void UpdateDownloadProgress(double value)
        {
            if (DownloadProgressChanged == null)
                return;
            DownloadProgressChanged(this, new DownloadProgressChangedEventArgs(value));
        }

        protected internal void UpdateDownloadStatus(DownloadStatus status)
        {
            if (DownloadStatusChanged == null)
                return;
            DownloadStatusChanged(this, new DownloadStatusChangedEventArgs(status, _fileDownload));
        }

        public static DownloadTask LoadSavedDownload(string tempFile)
        {
            StoredDownload download = DeserializeDownloadState(tempFile);
            return new DownloadTask(download.DownloadFolder, download.TempFolder, download.FileName, download.FileType, download.Source, download.DownloadStatus.ChunkSize, download.DownloadStatus.WrittenChunks, download.DownloadStatus.ContentLength);
        }

        protected static internal StoredDownload DeserializeDownloadState(string tempFile)
        {
            var deserialize = new DataContractSerializer(typeof(StoredDownload));
            return (StoredDownload)deserialize.ReadObject(new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }
    }

}
