using StreamDownloaderDownload.Hosts;
using System;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Download
{
    public class LinkFetchTask
    {
        #region C# variables and properties

        private Host _host;
        private string _link;

        public Host Host => _host;
        public string Link => _link;

        #endregion C# variables and properties

        public LinkFetchTask(Host host, string link)
        {
            _host = host;
            _link = link;
        }

        public async Task<Tuple<string, LinkFetchResult>> Fetch()
        {
            return await _host.GetSourceLink(_link);
        }
    }
}