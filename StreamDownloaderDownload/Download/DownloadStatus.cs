using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Download
{
    public enum DownloadStatus
    {
        COMPLETED,
        DOWNLOADING,
        PAUSED,
        CONTINUNING_LATER,
        ERROR
    }
}
