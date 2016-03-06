using System;
using System.Collections;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload
{
    public class StreamDownloader
    {
        private Hashtable _activeDownloads;

        public DownloadTask CreateNewDownload(string downloadFolder, string fileName, string source, DownloadTask.UpdateDownloadProgress updateDownloadProgress, DownloadTask.CompleteDownload completeDownload)
        {
            DownloadTask d = new DownloadTask(downloadFolder, fileName, source, updateDownloadProgress, completeDownload);
            d.Start();
        }
    }
}
