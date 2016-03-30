using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Download
{
    public class DownloadStartedEventArgs
    {
        public DownloadStartedEventArgs(DateTime time)
        {
            Time = time;
        }

        public DateTime Time { get; }
    }
}
