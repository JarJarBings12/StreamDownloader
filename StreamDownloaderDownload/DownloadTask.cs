using System;
using System.Net;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload
{
    public class DownloadTask
    {
        #region variables
        private readonly string _downloadFolder;
        private readonly string _fileName;
        private readonly string _absolutFilePath;
        private readonly string _source;
        private readonly UpdateDownloadProgress _progressChanged;
        private readonly CompleteDownload _downloadCompleted;
        private FileDownload _fileDownload;
        #endregion

        public DownloadTask(string downloadFolder, string fileName, string source, UpdateDownloadProgress updateDownloadProgress, CompleteDownload downloadComplete)
        {
            this._downloadFolder = downloadFolder;
            this._fileName = fileName;
            this._absolutFilePath = $"{_downloadFolder}\\{_fileName}";
            this._source = source;
            this._progressChanged = updateDownloadProgress;
            this._downloadCompleted = downloadComplete;
            this._fileDownload = new FileDownload(_source, _absolutFilePath, 20480, _progressChanged, _downloadCompleted);
        }

        #region properties
        public string DownloadFolder => _downloadFolder;
        public string FileName => _fileName;
        public string AbsolutFilePath => _absolutFilePath;
        public string Source => _source;
        public FileDownload Download => _fileDownload;
        #endregion

        public void Start()
        {
            _fileDownload.Start();
        }

        public void Pause()
        {
            _fileDownload.Pause();
        }

        public bool IsPaused()
        {
            return _fileDownload.IsPaused;
        }

        public void ContinuingLater()
        {
            _fileDownload.ContinuingLater();
        }

        /// <summary>
        /// Download progress update method delegation.
        /// </summary>
        /// <param name="progress"></param>
        public delegate void UpdateDownloadProgress(double progress);

        /// <summary>
        /// Download complete delegation.
        /// </summary>
        public delegate void CompleteDownload(FileDownload FileDownload);


    }
}
