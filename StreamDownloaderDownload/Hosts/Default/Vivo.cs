using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using mshtml;
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
        public sealed override Regex BaseUrlPattern => _BaseUrlPattern;
        public sealed override Regex SourceUrlPattern => _SourceUrlPattern;

        public sealed override bool NeedDelay => true;
        public sealed override int DelayInMilliseconds => 8000;
        #endregion

        public sealed override async Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url)
        {
            var ie = new IE();
            ie.Navigate2(url);

            while (ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
            { }

            SHDocVw.WebBrowser browser = ((SHDocVw.WebBrowser)ie); 
            HTMLDocument doc = ((HTMLDocument)browser.Document); //Get Document
            IHTMLElement button = doc.getElementById("access"); //Get HTML Button

            await Pause(9000); //Wait 9 Seconds.
            button.click(); //Click button

            await JavaScriptProcessingTime();

            UpdateStatus("Scanning Web page after link...");
            IHTMLElement element = GetStreamContent(((HTMLDocument)browser.Document)); //Get stream-content element from web page.

            var sourceUrl = string.Empty;
            var response = LinkFetchResult.SUCCESSFULL;
            if (element != null)
            {
                UpdateStatus("Starting download...");
                sourceUrl = element.getAttribute("data-url"); //Get data-url attribute
            }
            else
            {
                response = LinkFetchResult.FAILED;
                UpdateStatus("Source link not found");
            }
            
            return new Tuple<string, LinkFetchResult>(sourceUrl, response);
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
