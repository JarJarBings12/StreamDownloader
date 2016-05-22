using System;

namespace StreamDownloaderDownload.Download
{
    public class DownloadStartedEventArgs
    {
        public DateTime Time { get; }

        public DownloadStartedEventArgs(DateTime time)
        {
            Time = time;
        }
    }
}