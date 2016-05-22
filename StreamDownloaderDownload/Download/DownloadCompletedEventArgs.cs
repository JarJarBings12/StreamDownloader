namespace StreamDownloaderDownload.Download
{
    public class DownloadCompletedEventArgs
    {
        public FileDownload Download { get; }

        public DownloadCompletedEventArgs(FileDownload fileDownload)
        {
            Download = fileDownload;
        }
    }
}