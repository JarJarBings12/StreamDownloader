using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Hosters
{
    public class LinkFetchStatusChangedEventArgs
    {
        public LinkFetchStatusChangedEventArgs(string NewStatus)
        {
            this.Status = NewStatus;
        }

        public string Status { get; }
    }
}
