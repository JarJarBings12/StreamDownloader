using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Hosters
{
    public class LinkFetchFinishedEventArgs
    {
        public LinkFetchFinishedEventArgs(LinkFetchResult result)
        {
            Result = result;
        }

        public LinkFetchResult Result { get; }
    }
}
