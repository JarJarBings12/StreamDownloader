using mshtml;
using StreamDownloaderDownload.FileExtensions;
using StreamDownloaderDownload.FileExtensions.Default;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using IE = SHDocVw.InternetExplorer;

namespace StreamDownloaderDownload.Hosts.Default
{
    public class PowerWatch: Host
    {
        #region variables and properties

        public sealed override string HostName => "PowerWatch";
        public sealed override List<FileExtension> SupportedFileExtensions => new List<FileExtension>() { new MP4() };
        public override BitmapImage HostIcon => Host.BitmapToBitmapImage(Properties.Resources.PowerWatch);

        public sealed override Regex BaseUrlPattern => _baseUrlPattern;
        public sealed override Regex SourceUrlPattern => _bourceUrlPattern;

        public sealed override int DelayInMilliseconds => 6000;

        private readonly Regex _baseUrlPattern = new Regex(@"http://powerwatch.pw/(.*)");
        private readonly Regex _bourceUrlPattern = new Regex("sources:(.*)\\[\\{file:\"(.*)\",");
        private const int _javaScriptProcessingTime = 15;

        #endregion variables and properties

        public sealed override async Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url)
        {
            var ie = new IE();
            ie.Navigate2(url);

            while (ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
            { }

            SHDocVw.WebBrowser browser = ((SHDocVw.WebBrowser)ie);
            HTMLDocument doc = ((HTMLDocument)browser.Document);
            IHTMLElement button = doc.getElementById("btn_download");

            await Pause(5000); //Wait 5 Seconds.

            button.click(); //Click button

            string tempFile = $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.lf.html";

            await JavaScriptProcessingTime(); //Wait 15 seconds.

            using (var output = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] buffer = ASCIIEncoding.UTF8.GetBytes(((HTMLDocument)browser.Document).documentElement.outerHTML);
                await output.WriteAsync(buffer, 0, buffer.Length);
            }

            UpdateStatus("Scanning Web page after link...");

            var line = string.Empty;
            var input = new StreamReader(tempFile);
            Match match = null;

            while ((line = input.ReadLine()) != null) //Scan file for link.
            {
                if (line == "")
                    continue;
                match = _bourceUrlPattern.Match(line);

                if (match.Success)
                    break;
            }

            var sourceUrl = string.Empty;
            var response = LinkFetchResult.SUCCESSFULL;
            if (match.Success)
                sourceUrl = match.Groups[2].Value;
            else
                response = LinkFetchResult.FAILED;
            return new Tuple<string, LinkFetchResult>(sourceUrl, response);
        }

        public sealed override async Task Pause(int interval)
        {
            for (int i = (interval / 1000); i > 0; i--)
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