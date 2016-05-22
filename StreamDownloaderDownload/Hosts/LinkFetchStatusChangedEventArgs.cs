namespace StreamDownloaderDownload.Hosts
{
    public class LinkFetchStatusChangedEventArgs
    {
        public LinkFetchStatusChangedEventArgs(string message)
        {
            this.Message = message;
        }

        public string Message { get; }
    }
}