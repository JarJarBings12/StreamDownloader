namespace StreamDownloaderDownload.Download
{
    public class DownloadStatusChangedEventArgs
    {
        public DownloadStatusChangedEventArgs(string message, DownloadStatus status, FileDownload fileDownload)
        {
            Message = message;
            Status = status;
            Download = fileDownload;
        }

        public string Message { get; }

        public DownloadStatus Status { get; }

        public FileDownload Download { get; }
    }
}