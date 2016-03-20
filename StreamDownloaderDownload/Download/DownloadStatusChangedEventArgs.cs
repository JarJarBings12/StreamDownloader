using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Download
{
    public class DownloadStatusChangedEventArgs
    {
        public DownloadStatusChangedEventArgs(DownloadStatus status, FileDownload fileDownload)
        {
            Status = status;
            Download = fileDownload;
        }

        public DownloadStatus Status { get; }

        public FileDownload Download { get; }
    }
}
