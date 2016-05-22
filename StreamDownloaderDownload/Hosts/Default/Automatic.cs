using StreamDownloaderDownload.FileExtensions;
using StreamDownloaderDownload.FileExtensions.Default;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StreamDownloaderDownload.Hosts.Default
{
    public class Automatic: Host
    {
        public sealed override string HostName => "Automatic";
        public sealed override List<FileExtension> SupportedFileExtensions => new List<FileExtension>() { new MP4() };
        public sealed override BitmapImage HostIcon => Host.BitmapToBitmapImage(Properties.Resources.PowerWatch);
        public sealed override Regex BaseUrlPattern => null;
        public sealed override Regex SourceUrlPattern => null;
        public override int DelayInMilliseconds => -1;

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        public sealed override async Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            return new Tuple<string, LinkFetchResult>("N/A", LinkFetchResult.SUCCESSFULL);
        }
    }
}