using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StreamDownloaderDownload.Hosts.Default
{
    public class Automatic: Host
    {
        public override Regex BaseUrlPattern => null;
        public override Regex SourceUrlPattern => null;

        public override int DelayInMilliseconds => -1;
        public override bool NeedDelay => false;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public sealed override async Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new Tuple<string, LinkFetchResult>("N/A", LinkFetchResult.SUCCESSFULL);
        }
    }
}
