using System;
using System.Windows.Forms;
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
        private volatile bool _pause = false;
        private volatile bool _continunigLater = false;

        private readonly string _source;
        private readonly string _file;
        private readonly string _tempFile;

        private readonly DownloadTask.UpdateDownloadProgress _updateDownloadProgress;
        private readonly DownloadTask.CompleteDownload _completeDownload;

        /* Max 32 bit (uint) */
        private uint _chunkSize;
        /* Max 32 bit (uint) */
        private Lazy<ulong> _contentLength { get; set; }
        /* Max 16 bit (ushort) */
        private int _writtenChunks { get; set; }
        #endregion

        public FileDownload(string source, string file, ushort chunkSize, DownloadTask.UpdateDownloadProgress updateDownloadProgress, DownloadTask.CompleteDownload completeDownload)
        {
            this._source = source;
            this._file = file;
            _tempFile = @"C:\Users\tobias\Downloads\ad.mp4";
            this._chunkSize = chunkSize;
            this._contentLength = new Lazy<ulong>(() => Convert.ToUInt64(GetContentLength()));
            this._writtenChunks = 0;
            this._updateDownloadProgress = updateDownloadProgress;
            this._completeDownload = completeDownload;
        }

        public FileDownload(string source, string file, ushort chunkSize, ushort writtenChunks, DownloadTask.UpdateDownloadProgress updateDownloadProgress, DownloadTask.CompleteDownload completeDownload)
        {
            this._source = source;
            this._file = file;
            this._tempFile = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString();
            this._chunkSize = chunkSize;
            this._contentLength = new Lazy<ulong>(() => Convert.ToUInt64(GetContentLength()));
            this._writtenChunks = writtenChunks;
            this._updateDownloadProgress = updateDownloadProgress;
            this._completeDownload = completeDownload;
        }

        #region properties
        public string Source => _source;
        public string FileDestination => _file;
        public string TempFile => _tempFile;

        public uint ChunkSize => _chunkSize;
        public Lazy<ulong> ContentLength => _contentLength;
        public int WrittenChunks => _writtenChunks;

        public bool Finished => ((ulong)_writtenChunks) == _contentLength.Value;
        public bool IsPaused => _pause;
        #endregion

        private long GetContentLength()
        {
            var request = (HttpWebRequest)WebRequest.Create(new Uri(_source));
            request.Method = "HEAD";

            using (var response = request.GetResponse())
                return response.ContentLength;
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
            request.ContentType = "application/x-www-form-urlencoded";
            request.AddRange(writtenChunks);

            if (!File.Exists(_tempFile))
                using (File.Create(_tempFile));

            //Wait until the server response
            using (var response = await request.GetResponseAsync())
            {
                using (var responseStream = response.GetResponseStream())
                {
                    try
                    {
                        using (var tempFileStream = new FileStream(_tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                        {
                            byte[] buffer;
                            ulong length = _contentLength.Value;
                            int readBytes = 0;

                            while ((length > _chunkSize) && !_pause && !_continunigLater)
                            {
                                buffer = new byte[_chunkSize];
                                readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length);

                                await tempFileStream.WriteAsync(buffer, 0, readBytes);
                                length -= (ulong)readBytes;
                                writtenChunks += readBytes;
                            }

                            if ((length > _chunkSize) && !_pause && !_continunigLater)
                            {
                                buffer = new byte[length];
                                readBytes = await responseStream.ReadAsync(buffer, 0, buffer.Length);
                                writtenChunks += readBytes;
                                await tempFileStream.WriteAsync(buffer, 0, readBytes);
                            }

                            if (Finished)
                            {
                                _completeDownload(this);
                                if (!File.Exists(_file))
                                    File.Move(_tempFile, _file);

                                if (File.Exists(_tempFile))
                                    File.Delete(_tempFile);
                                if (File.Exists(_file))
                                    File.Delete(_file);
                            }

                            /* Write current download status to file to continue later. */
                            if (_continunigLater || _pause )
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
                                    buffer = BitConverter.GetBytes((uint)_contentLength.Value);
                                    await continingStream.WriteAsync(buffer, 0, buffer.Length);
                                }
                            }

                            await tempFileStream.FlushAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message);
                    }
                }
            }
        }

        /*----*/

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
