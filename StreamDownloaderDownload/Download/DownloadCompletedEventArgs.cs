using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Download
{
    public class DownloadCompletedEventArgs
    {
        public DownloadCompletedEventArgs(FileDownload fileDownload)
        {
            Download = fileDownload;
        }

        public FileDownload Download { get; }
    }
}
