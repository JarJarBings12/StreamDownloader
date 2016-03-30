using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.Hosters;

namespace StreamDownloaderDownload.Download
{
    public class LinkFetchTask
    {
        private Hoster _host;
        private string _link;

        public LinkFetchTask(Hoster host, string link)
        {
            _host = host;
            _link = link;
        }

        public Hoster Host => _host;
        public string Link => _link;

        public async Task<string> Fetch()
        {
            return await _host.GetSourceLink(_link);
        }
    }
}
