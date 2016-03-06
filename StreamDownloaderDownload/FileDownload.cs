using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload
{
    public class FileDownload
    {

        #region variables
        private volatile bool _pause;
        private volatile bool _continunigLater;

        private readonly string _source;
        private readonly string _file;
        private readonly string _tempFile;

        private readonly DownloadTask.UpdateDownloadProgress _updateDownloadProgress;
        private readonly DownloadTask.CompleteDownload _completeDownload;

        /* Max 32 bit (uint) */
        private uint _chunkSize;
        /* Max 32 bit (uint) */
        private uint _contentLength { get; set; }
        /* Max 16 bit (ushort) */
        private ushort _writtenChunks { get; set; }
        #endregion

        public FileDownload(string source, string file, ushort chunkSize, DownloadTask.UpdateDownloadProgress updateDownloadProgress, DownloadTask.CompleteDownload completeDownload)
        {
            this._source = source;
            this._file = file;
            _tempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();
            this._chunkSize = chunkSize;
            this._contentLength = GetContentLength();
            this._writtenChunks = 0;
            this._updateDownloadProgress = updateDownloadProgress;
            this._completeDownload = completeDownload;
        }

        public FileDownload(string source, string file, ushort chunkSize, ushort writtenChunks, DownloadTask.UpdateDownloadProgress updateDownloadProgress, DownloadTask.CompleteDownload completeDownload)
        {
            this._source = source;
            this._file = file;
            _tempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();
            this._chunkSize = chunkSize;
            this._contentLength = GetContentLength();
            this._writtenChunks = writtenChunks;
            this._updateDownloadProgress = updateDownloadProgress;
            this._completeDownload = completeDownload;
        }

        #region properties
        public string Source => _source;
        public string File => _file;
        public string TempFile => _tempFile;

        public uint ChunkSize => _chunkSize;
        public uint ContentLength => _contentLength;
        public ushort WrittenChunks => _writtenChunks;

        public bool Finished => _contentLength == _writtenChunks;
        public bool IsPaused => _pause;
        #endregion

        private uint GetContentLength()
        {
            var request = (HttpWebRequest)WebRequest.Create(_source);
            request.Method = "HEAD";

            using (var response = request.GetResponse())
                return (uint) Convert.ToInt32(response.ContentLength);
        }

        public delegate void UpdateDownloadProgress(double progress);

        public delegate void CompleteDownload(FileDownload downloadedFile);

        public async Task BeginDownload(int writtenChunks)
        {
            if (_pause)
                throw new InvalidOperationException();

            var request = (HttpWebRequest)WebRequest.Create(_source);
            request.Method = "GET";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
            request.AddRange(writtenChunks);

            //Wait until the server response
            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    using (var tempFileStream = new FileStream(_tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    {
                        byte[] buffer = new byte[_chunkSize];
                        ushort readBytes = 0;
                        while (!_pause && !_continunigLater)
                        {
                            readBytes = (ushort) await responseStream.ReadAsync(buffer, 0, buffer.Length);

                            if (readBytes == 0)
                                break;

                            await tempFileStream.WriteAsync(buffer, 0, buffer.Length);
                            _writtenChunks += readBytes;
                            Array.Clear(buffer, 0, buffer.Length);
                            _updateDownloadProgress((100 / _contentLength) * _writtenChunks);
                        }

                        /* Write current download status to file to continue later. */
                        if (_continunigLater && !(_writtenChunks == _contentLength))
                        {
                            using (var continingStream = new FileStream($"{_tempFile}.stpd", FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                            {
                                /* Content file */
                                buffer = UTF8Encoding.UTF8.GetBytes(_tempFile);
                                await continingStream.WriteAsync(buffer, 0, buffer.Length);
                                /* Written chunks */
                                buffer = BitConverter.GetBytes((ushort)_writtenChunks);
                                await continingStream.WriteAsync(buffer, 0, buffer.Length);
                                /* Chunk size - buffer size */
                                buffer = BitConverter.GetBytes((uint)_chunkSize);
                                await continingStream.WriteAsync(buffer, 0, buffer.Length);
                                /* Full content length */
                                buffer = BitConverter.GetBytes((uint)_contentLength);
                                await continingStream.WriteAsync(buffer, 0, buffer.Length);
                            }
                        }
                        
                        if (Finished)
                        {
                            _completeDownload(this);
                        }

                        await tempFileStream.FlushAsync();
                    }
                }
            }
        }

        public Task Start()
        {
            this._pause = false;
            return this.BeginDownload(_writtenChunks);
        }

        public void Pause()
        {
            this._pause = true;
        }

        public void ContinuingLater()
        {
            this._continunigLater = true;
        }
    }
}
