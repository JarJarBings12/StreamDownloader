using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Download
{
    public class DownloadProgressChangedEventArgs
    {
        public DownloadProgressChangedEventArgs(double value)
        {
            Progress = value;
        }

        public double Progress { get; }
    }
}
