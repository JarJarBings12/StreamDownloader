using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Download
{
    public class DownloadProgressChangedEventArgs
    {
        public DownloadProgressChangedEventArgs(double value, ulong writtenBytes)
        {
            Progress = value;
            WrittenBytes = writtenBytes;
        }

        public double Progress { get; }

        public ulong WrittenBytes { get; }
    }
}
