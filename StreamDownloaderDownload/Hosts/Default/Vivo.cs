using mshtml;
using StreamDownloaderDownload.FileExtensions;
using StreamDownloaderDownload.FileExtensions.Default;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Media.Imaging;
using IE = SHDocVw.InternetExplorer;

namespace StreamDownloaderDownload.Hosts.Default
{
    public class Vivo: Host
    {
        #region variables and properties

        private readonly Regex _BaseUrlPattern = new Regex(@"http://vivo.sx/(.*)");
        private readonly Regex _SourceUrlPattern = new Regex("(.*)<div\\s*class=\"stream-content\"\\s*data-url=\"(.*)\"\\s*data-name=\"(.*)\"\\s*data-title=\"(.*)\"\\s.*data-poster=\"(.*)\"\\s*style=\"(.*)\">");
        private const int _JavaScriptProcessingTime = 15;

        /* Properties */
        public sealed override string HostName => "Vivo";
        public sealed override List<FileExtension> SupportedFileExtensions => new List<FileExtension>() { new MP4() };
        public sealed override BitmapImage HostIcon => Host.BitmapToBitmapImage(Properties.Resources.Vivo);

        public sealed override Regex BaseUrlPattern => _BaseUrlPattern;
        public sealed override Regex SourceUrlPattern => _SourceUrlPattern;

        public sealed override int DelayInMilliseconds => 8000;

        #endregion variables and properties

        public sealed override async Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url)
        {
            var ie = new IE();
            ie.Navigate2(url);

            while (ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
            { }

            SHDocVw.WebBrowser browser = ((SHDocVw.WebBrowser)ie);
            HTMLDocument doc = ((HTMLDocument)browser.Document); //Get Document

            IHTMLElementCollection searchHash = (IHTMLElementCollection)doc.getElementsByName("hash");

            string hash = string.Empty;
            string timestamp = string.Empty;

            foreach (IHTMLElement element in searchHash)
            {
                if (element == null)
                    break;
                if ((string)element.getAttribute("name") == "hash")
                    hash = element.getAttribute("value");
            }

            IHTMLElementCollection searchTimestamp = (IHTMLElementCollection)doc.getElementsByName("timestamp");

            foreach (IHTMLElement element in searchTimestamp)
            {
                if (element == null)
                    break;
                if ((string)element.getAttribute("name") == "timestamp")
                    timestamp = element.getAttribute("value");
            }

            /* PREPARE POST DATA */

            var stringData = $"hash={HttpUtility.UrlEncode(hash)}&timestamp={HttpUtility.UrlEncode(timestamp)}";
            var byteData = Encoding.ASCII.GetBytes(stringData);

            /* PREPARE HTTP CLIENT */
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/42.0.2311.135 Safari/537.36 Edge/12.246";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteData.Length;

            /* SEND POST DATA ASYNC */

            Stream requestStream = await request.GetRequestStreamAsync();
            await requestStream.WriteAsync(byteData, 0, byteData.Length);
            requestStream.Close();

            /* RECIVE RESPONSE ASYNC */
            var response = ((HttpWebResponse)await request.GetResponseAsync()).GetResponseStream();
            var streamReader = new StreamReader(response, Encoding.Default);

            var responseString = await streamReader.ReadToEndAsync();

            /* PARSE STRING TO DOC  */
            UpdateStatus("Scanning Web page after link");
            var docTemp = (IHTMLDocument2)doc;
            docTemp.write(responseString);

            /* SEARCH DOWNLOAD LINK */
            var contentElement = GetStreamContent(doc);

            var sourceURL = string.Empty;
            var fetchResponse = LinkFetchResult.SUCCESSFULL;

            if (contentElement != null)
            {
                UpdateStatus("Starting download...");
                sourceURL = contentElement.getAttribute("data-url"); //Get data-url attribute
            }
            else
            {
                fetchResponse = LinkFetchResult.FAILED;
                UpdateStatus("Source link not found");
            }

            ie.Quit();
            return new Tuple<string, LinkFetchResult>(sourceURL, fetchResponse);
        }

        public IHTMLElement GetStreamContent(HTMLDocument doc)
        {
            foreach (IHTMLElement element in doc.all)
                if (element.getAttribute("className") == "stream-content")
                    return ((IHTMLElement)element);
            return null;
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
            for (int i = _JavaScriptProcessingTime; i > 0; i--)
            {
                UpdateStatus($"Download begins in { i }");
                await Task.Delay(1000);
            }
            return;
        }
    }
}