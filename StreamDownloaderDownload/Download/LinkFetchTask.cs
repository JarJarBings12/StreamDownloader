using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.Hosts;

namespace StreamDownloaderDownload.Download
{
    public class LinkFetchTask
    {
        private Host _host;
        private string _link;

        public LinkFetchTask(Host host, string link)
        {
            _host = host;
            _link = link;
        }

        public Host Host => _host;
        public string Link => _link;

        public async Task<string> Fetch()
        {
            return await _host.GetSourceLink(_link);
        }
    }
}
