namespace StreamDownloaderDownload.Download
{
    public enum DownloadStatus
    {
        NOT_INITIALIZED = 0,
        INITIALIZE = 10,
        DOWNLOADING = 20,
        PAUSED = 21,
        CONTINUNING_LATER = 22,
        COMPLETED = 30,
        CANCELLED = 31,
        ERROR = 32
    }
}