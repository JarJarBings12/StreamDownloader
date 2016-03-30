using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Hosters
{
    public delegate void FetchStatusChangedHandler(object sender, LinkFetchStatusChangedEventArgs e);
    public delegate void LinkFetchFinshedHandler(object sender, LinkFetchFinishedEventArgs e);

    public abstract class Hoster
    {
        public abstract Regex BaseUrlPattern { get; }
        public abstract Regex SourceUrlPattern { get; }

        public abstract bool NeedDelay { get; }
        public abstract int DelayInMilliseconds { get; }

        public event FetchStatusChangedHandler FetchStatusChanged;
        public event LinkFetchFinshedHandler LinkFetchFinished;

        public void UpdateStatus(string NewStatus)
        {
            if (FetchStatusChanged == null)
                return;
            FetchStatusChanged(this, new LinkFetchStatusChangedEventArgs(NewStatus));
        }

        public void SetLinkFetchResultTo(LinkFetchResult result)
        {
            if (LinkFetchFinished == null)
                return;
            LinkFetchFinished(this, new LinkFetchFinishedEventArgs(result));
        }

        public abstract Task<string> GetSourceLink(string url);

        /* Asynchronous pause */
        public virtual async Task Pause(int interval)
        {
            await Task.Delay(interval);
        }
    }
}
