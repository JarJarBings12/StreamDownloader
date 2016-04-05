using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
