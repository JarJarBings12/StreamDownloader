using mshtml;
using SHDocVw;
using StreamDownloaderDownload.FileExtensions;
using StreamDownloaderDownload.FileExtensions.Default;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Browser = SHDocVw.WebBrowser;
using IE = SHDocVw.InternetExplorer;

namespace StreamDownloaderDownload.Hosts.Default
{
    public class StreamCloud: Host
    {
        #region variables and properties

        /* Properties */
        public sealed override string HostName => "StreamCloud";
        public sealed override List<FileExtension> SupportedFileExtensions => new List<FileExtension>() { new MP4() };
        public sealed override BitmapImage HostIcon => Host.BitmapToBitmapImage(Properties.Resources.StreamCloud);
        public sealed override Regex BaseUrlPattern => _baseUrlPattern;
        public sealed override Regex SourceUrlPattern => _sourceUrlPattern;

        public sealed override int DelayInMilliseconds => 11000;

        private readonly Regex _baseUrlPattern = new Regex(@"http://streamcloud.eu/(.*)/(.*)");
        private readonly Regex _sourceUrlPattern = new Regex("file:\\s*\\\"(.*)\\\"");
        private const int _javaScriptProcessingTime = 15;

        #endregion variables and properties

        public sealed override async Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url)
        {
            var ie = new IE(); //Create new browser (IE)
            ie.Navigate2(url);

            while (ie.ReadyState != tagREADYSTATE.READYSTATE_COMPLETE)
            { }

            await Pause(DelayInMilliseconds);

            Browser browser = ((Browser)ie);
            HTMLDocument doc = ((HTMLDocument)browser.Document); //Get document
            IHTMLElement button = doc.getElementById("btn_download"); //Define HTML button

            button.click();

            string tempFile = $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.lf.html";

            await JavaScriptProcessingTime();
            doc = ((HTMLDocument)browser.Document);

            using (var output = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] buffer = ASCIIEncoding.UTF8.GetBytes(((HTMLDocument)browser.Document).documentElement.outerHTML);
                await output.WriteAsync(buffer, 0, buffer.Length);
            }

            var line = string.Empty;
            var input = new StreamReader(tempFile);
            Match match = null;

            while ((line = input.ReadLine()) != null) //Scan file for link.
            {
                if (line == "")
                    continue;
                match = _sourceUrlPattern.Match(line);

                if (match.Success)
                    break;
            }

            var sourceUrl = string.Empty;
            var response = LinkFetchResult.SUCCESSFULL;

            if (match.Success)
                sourceUrl = match.Groups[1].Value;
            else
                response = LinkFetchResult.FAILED;
            return new Tuple<string, LinkFetchResult>(sourceUrl, response);
        }

        public sealed override async Task Pause(int mills)
        {
            for (int i = (mills / 1000) - 1; i > 0; i--)
            {
                UpdateStatus($"Download link will be made available in { i } seconds.");
                await Task.Delay(1000);
            }
            await Task.Delay(1000);
        }

        public async Task JavaScriptProcessingTime()
        {
            for (int i = _javaScriptProcessingTime; i > 0; i--)
            {
                UpdateStatus($"Download begins in { i }");
                await Task.Delay(1000);
            }
            return;
        }
    }
}