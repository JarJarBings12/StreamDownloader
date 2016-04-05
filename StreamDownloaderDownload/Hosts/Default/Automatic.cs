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

        public sealed override async Task<string> GetSourceLink(string url)
        {
            return "N/A";
        }
    }
}
