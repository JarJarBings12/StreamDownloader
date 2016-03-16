using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using mshtml;

namespace StreamDownloaderDownload.Hosters.Default
{
    public class Vivo: Hoster
    {
        #region variables and properties
        private readonly Regex _BaseUrlPattern = new Regex(@"http://vivo.sx/(.*)");
        private readonly Regex _SourceUrlPattern = new Regex("(.*)<div\\s*class=\"stream-content\"\\s*data-url=\"(.*)\"\\s*data-name=\"(.*)\"\\s*data-title=\"(.*)\"\\s.*data-poster=\"(.*)\"\\s*style=\"(.*)\">");
        private const int _JavaScriptProcessingTime = 15;

        /* Properties */
        public sealed override Regex BaseUrlPattern => _BaseUrlPattern;
        public sealed override Regex SourceUrlPattern => _SourceUrlPattern;

        public sealed override bool NeedDelay => true;
        public sealed override int DelayInMilliseconds => 8000;
        #endregion

        public sealed override async Task<string> GetSourceLink(string url)
        {
            SHDocVw.InternetExplorer ie = new SHDocVw.InternetExplorer();
            ie.Navigate2(url);
            ie.Visible = true; //TODO Remove

            while (ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
            { }

            SHDocVw.WebBrowser browser = ((SHDocVw.WebBrowser)ie); 
            HTMLDocument doc = ((HTMLDocument)browser.Document); //Get Document
            IHTMLElement button = doc.getElementById("access"); //Get HTML Button

            await Pause(9); //Wait 8 Seconds.
            button.click(); //Click button

            string tempFile = $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.lf.html";

            await JavaScriptProcessingTime();

            using (var output = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] buffer = ASCIIEncoding.UTF8.GetBytes(((HTMLDocument)browser.Document).documentElement.outerHTML);
                await output.WriteAsync(buffer, 0, buffer.Length);
            }

            UpdateStatus("Scanning Web page after link...");

            IHTMLElement element = GetStreamContent(((HTMLDocument)browser.Document));

            if (element != null)
            {
                UpdateStatus("Starting download...");
                SetLinkFetchResultTo(LinkFetchResult.SUCCESSFULL);
                return element.getAttribute("data-url");
            }
            UpdateStatus("Source link not found");
            SetLinkFetchResultTo(LinkFetchResult.CANCELED);
            return element.getAttribute("");
        }

        public sealed override async Task Pause(int mills)
        {
            for (int i = (mills / 1000) - 1; i > 0; i--)
            {
                UpdateStatus($"Download link will be made available in { (i / 1000) } seconds.");
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

        public IHTMLElement GetStreamContent(HTMLDocument doc)
        {
            foreach (IHTMLElement element in doc.all)
                if (element.getAttribute("className") == "stream-content")
                    return ((IHTMLElement)element);
            return null;
        }
    }
}
