using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using StreamDownloaderDownload.Download.JSON;

namespace StreamDownloaderDownload.Download
{
    public class FileDownload
    {

        #region variables and properties
        //Public
        public string Source => _status;
        public string FileDestination => _File;
        public string TempFile => _tmpFile;

        public uint ChunkSize => _bufferSize;
        public Lazy<ulong> ContentLength => _contentLength;
        public ulong WrittenBytes => _writtenBytes;

        public bool Finished => _writtenBytes == _contentLength.Value;
        public bool IsPaused => _pause;

        // Private
        private volatile bool _pause = false;
        private volatile bool _continunigLater = false;
        private volatile bool _cancel = false;

        private readonly string _status;
        private readonly string _File;
        private readonly string _tmpFile;
        private readonly DownloadTask _task;

        private uint _bufferSize;
        private Lazy<ulong> _contentLength;
        private ulong _writtenBytes;
        #endregion

        #region Constructor
        public FileDownload(string source, string tempFile, string file, uint bufferSize, DownloadTask task)
        {
            _status = source;
            _tmpFile = tempFile;
            _File = file;
            _bufferSize = bufferSize;
            _writtenBytes = 0;
            _contentLength = new Lazy<ulong>(() => Convert.ToUInt64(GetContentLength()));
            _task = task;
        }

        public FileDownload(string source, string tempFile, string file, uint bufferSize, ulong writtenBytes, ulong contentLength, DownloadTask task)
        {
            _status = source;
            _tmpFile = tempFile;
            _File = file;
            _bufferSize = bufferSize;
            _writtenBytes = writtenBytes;
            _contentLength = new Lazy<ulong>(() => Convert.ToUInt64(GetContentLength()));
            _task = task;
        }

        #endregion

        public Task Start()
        {
            _pause = false;
            _task.StartDownload();
            return this.BeginDownload(_writtenBytes);
        }

        public void Pause()
        {
            this._pause = true;
        }

        public void Cancel()
        {
            this._cancel = true;
        }

        public void ContinuingLater()
        {
            this._continunigLater = true;
        }

        public async Task BeginDownload(ulong writtenBytes)
        {
            if (_pause)
                throw new InvalidOperationException();

            var request = (HttpWebRequest)WebRequest.Create(_status);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AddRange((long)_writtenBytes);

            if (!File.Exists(_tmpFile))
                using (File.Create(_tmpFile)) ;

            //Wait until the server response
            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    try
                    {
                        _task.StartDownload();
                        _task.UpdateDownloadStatus("Download...", DownloadStatus.DOWNLOADING);
                        using (var tempFileStream = new FileStream(_tmpFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            byte[] buffer;
                            ulong length = _contentLength.Value;
                            int readBytes = 0;

                            /* Use buffer size X until the remaining bytes (length) are smaller than X. */
                            while ((length > _bufferSize) && !_pause && !_continunigLater && !_cancel)
                            {
                                buffer = new byte[_bufferSize];
                                readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length);

                                await tempFileStream.WriteAsync(buffer, 0, readBytes);
                                length -= (ulong)readBytes;
                                _writtenBytes += (uint)readBytes;
                            }

                            /*  Set buffer size to the size of the remaining bytes. */
                            if ((length < _writtenBytes) && !_pause && !_continunigLater && !_cancel)
                            {
                                buffer = new byte[length];
                                readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                                _writtenBytes += (uint)readBytes;
                                length -= (ulong)readBytes;
                                await tempFileStream.WriteAsync(buffer, 0, readBytes);
                                _writtenBytes = (ulong)_contentLength.Value;
                            }

                            //Flush
                            await tempFileStream.FlushAsync();
                        }

                        _task.ShutdownDownload();
                        if (Finished)
                        {
                            _task.UpdateDownloadStatus("Download complete.", DownloadStatus.COMPLETED);
                            if (!File.Exists(_File))
                                File.Move(_tmpFile, _File);

                            if (File.Exists(_tmpFile))
                                File.Delete(_tmpFile);
                        }

                        /* Set download status to pause and exit */
                        if (_pause)
                        {
                            _task.UpdateDownloadStatus("Download paused...", DownloadStatus.PAUSED);
                            return;
                        }

                        /* Set download status to cancelled, delete files and exit */
                        if (_cancel)
                        {
                            _task.UpdateDownloadStatus("Download cancelled!", DownloadStatus.CANCELLED);
                            if (File.Exists(_tmpFile))
                                File.Delete(_tmpFile);
                            if (File.Exists(_File))
                                File.Delete(_File);                        
                            return;
                        }

                        /* Write current download status to file to continue later. */
                        if (_continunigLater)
                        {
                            var empty = (_pause == true) ? new Action(() => _task.UpdateDownloadStatus("Paused...", DownloadStatus.PAUSED)) : new Action(() => _task.UpdateDownloadStatus("Continuing later.", DownloadStatus.CONTINUNING_LATER));

                            using (var downloadRestoreFileStream = new FileStream($"{_tmpFile}.pdi", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            {
                                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(StoredDownload));
                                jsonSerializer.WriteObject(downloadRestoreFileStream, SerializeDownloadStatus());
                            }
                            _task.UpdateDownloadStatus("Download saved...", DownloadStatus.CONTINUNING_LATER);
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                        _task.UpdateDownloadStatus("Download failed!", DownloadStatus.ERROR);
                    }
                }
            }
        }

        private long GetContentLength()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(_status));
            request.Method = "HEAD";

            using (var response = request.GetResponse())
                return response.ContentLength;
        }

        public StoredDownload SerializeDownloadStatus()
        {
            StoredDownload storedDownload = new StoredDownload()
            {
                TempFolder = StreamDownloader.DownloadTempFolder,
                FileName = _task.FileName,
                FileType = _task.FileType,
                DownloadFolder = _task.DownloadFolder,
                RawUrl = _task.RawDownloadUrl,
                SourceUrl = _status,

                DownloadStatus = new StreamDownloaderDownload.Download.JSON.DownloadStatus()
                {
                    WrittenChunks = _writtenBytes,
                    ChunkSize = _bufferSize,
                    ContentLength = _contentLength.Value
                }
            };
            return storedDownload;
        }
    }
}
