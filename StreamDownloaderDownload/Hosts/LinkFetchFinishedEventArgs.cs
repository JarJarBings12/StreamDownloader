namespace StreamDownloaderDownload.Hosts
{
    public class LinkFetchFinishedEventArgs
    {
        public LinkFetchResult Result { get; }

        public LinkFetchFinishedEventArgs(LinkFetchResult result)
        {
            Result = result;
        }
    }
}