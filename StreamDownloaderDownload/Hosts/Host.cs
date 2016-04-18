using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Hosts
{
    public delegate void FetchStatusChangedHandler(object sender, LinkFetchStatusChangedEventArgs e);

    public abstract class Host
    {
        public abstract Regex BaseUrlPattern { get; }
        public abstract Regex SourceUrlPattern { get; }

        public abstract bool NeedDelay { get; }
        public abstract int DelayInMilliseconds { get; }

        public event FetchStatusChangedHandler FetchStatusChanged;

        public void UpdateStatus(string NewStatus)
        {
            if (FetchStatusChanged == null)
                return;
            FetchStatusChanged(this, new LinkFetchStatusChangedEventArgs(NewStatus));
        }

        public abstract Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url);

        /* Asynchronous pause */
        public virtual async Task Pause(int interval)
        {
            await Task.Delay(interval);
        }
    }
}
