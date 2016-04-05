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

        #region variables
        private volatile bool _pause = false;
        private volatile bool _continunigLater = false;

        private readonly string _source;
        private readonly string _file;
        private readonly string _tempFile;

        private readonly DownloadTask _task;

        /* Max 32 bit (uint) */
        private uint _chunkSize;
        /* Max 64 bit (uint) */
        private Lazy<ulong> _contentLength { get; set; }
        /* Max 64 bit (ulong) */
        private ulong _writtenBytes { get; set; }
        #endregion

        #region Constructor
        public FileDownload(string source, string tempFile, string file, uint chunkSize, DownloadTask task)
        {
            _source = source;
            _tempFile = tempFile;
            _file = file;
            _chunkSize = chunkSize;
            _writtenBytes = 0;
            _contentLength = new Lazy<ulong>(() => Convert.ToUInt64(GetContentLength()));
            _task = task;
        }

        public FileDownload(string source, string tempFile, string file, uint chunkSize, ulong writtenBytes, ulong contentLength, DownloadTask task)
        {
            _source = source;
            _tempFile = tempFile;
            _file = file;
            _chunkSize = chunkSize;
            _writtenBytes = writtenBytes;
            _contentLength = new Lazy<ulong>(() => Convert.ToUInt64(GetContentLength()));
            _task = task;
        }

        #endregion

        #region properties
        public string Source => _source;
        public string FileDestination => _file;
        public string TempFile => _tempFile;

        public uint ChunkSize => _chunkSize;
        public Lazy<ulong> ContentLength => _contentLength;
        public ulong WrittenBytes => _writtenBytes;

        public bool Finished => _writtenBytes == _contentLength.Value;
        public bool IsPaused => _pause;
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

        public void ContinuingLater()
        {
            this._continunigLater = true;
        }

        public async Task BeginDownload(ulong writtenChunks)
        {
            if (_pause)
                throw new InvalidOperationException();

            var request = (HttpWebRequest)WebRequest.Create(_source);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
            request.ContentType = "application/x-www-form-urlencoded";
            request.AddRange((long)_writtenBytes);

            if (!File.Exists(_tempFile))
                using (File.Create(_tempFile))

            //Wait until the server response
            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    try
                    {
                        _task.UpdateDownloadStatus("Download...", DownloadStatus.DOWNLOADING);
                        _task.StartDownload();
                        using (var tempFileStream = new FileStream(_tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            byte[] buffer;
                            ulong length = _contentLength.Value;
                            int readBytes = 0;

                            /* Use buffer size X until the remaining bytes (length) are smaller than X. */
                            while ((length > _chunkSize) && !_pause && !_continunigLater)
                            {
                                buffer = new byte[_chunkSize];
                                readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length);

                                await tempFileStream.WriteAsync(buffer, 0, readBytes);
                                length -= (ulong)readBytes;
                                _writtenBytes += (uint)readBytes;
                            }

                            /*  Set buffer size to the size of the remaining bytes. */
                            if ((length < _chunkSize) && !_pause && !_continunigLater)
                            {
                                buffer = new byte[length];
                                readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                                writtenChunks += (uint)readBytes;
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
                            if (!File.Exists(_file))
                                File.Move(_tempFile, _file);

                            if (File.Exists(_tempFile))
                                File.Delete(_tempFile);
                        }

                        /* Write current download status to file to continue later. */
                        if (_continunigLater || _pause)
                        {
                            var empty = (_pause == true) ? new Action(() => _task.UpdateDownloadStatus("Paused...", DownloadStatus.PAUSED)) : new Action(() => _task.UpdateDownloadStatus("Continuing later.", DownloadStatus.CONTINUNING_LATER));

                            using (var downloadRestoreFileStream = new FileStream($"{_tempFile}.pdi", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            {
                                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(StoredDownload));
                                jsonSerializer.WriteObject(downloadRestoreFileStream, SerializeDownloadStatus());
                            }
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
            var request = (HttpWebRequest)WebRequest.Create(new Uri(_source));
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

                DownloadStatus = new StreamDownloaderDownload.Download.JSON.DownloadStatus()
                {
                    WrittenChunks = _writtenBytes,
                    ChunkSize = _chunkSize,
                    ContentLength = _contentLength.Value
                }
            };
            return storedDownload;
        }
    }
}
