namespace StreamDownloaderDownload.Download
{
    public class DownloadProgressChangedEventArgs
    {
        public double Progress { get; }

        public ulong WrittenBytes { get; }

        public DownloadProgressChangedEventArgs(double value, ulong writtenBytes)
        {
            Progress = value;
            WrittenBytes = writtenBytes;
        }
    }
}