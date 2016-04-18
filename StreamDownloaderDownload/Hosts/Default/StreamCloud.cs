using System;
using System.Net;
using System.Drawing;
using System.Net.Http;
using System.Net.Security;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Web;
using SHDocVw;
using mshtml;
using IE = SHDocVw.InternetExplorer;

namespace StreamDownloaderDownload.Hosts.Default
{
    public class StreamCloud: Host
    {

        #region variables and properties 
        private readonly Regex _baseUrlPattern = new Regex(@"http://streamcloud.eu/(.*)/(.*)");
        private readonly Regex _sourceUrlPattern = new Regex("file:\\s*\\\"(.*)\\\"");
        private const int _javaScriptProcessingTime = 15;
        
        /* Properties */
        public override Regex BaseUrlPattern => _baseUrlPattern;
        public override Regex SourceUrlPattern => _sourceUrlPattern;

        public override bool NeedDelay => true;
        public override int DelayInMilliseconds => 11000;
        #endregion

        public sealed override async Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url)
        {
            var ie = new IE(); //Create new browser (IE)
            ie.Navigate2(url);
            
            while (ie.ReadyState != tagREADYSTATE.READYSTATE_COMPLETE)
            { }
            
            await Pause(DelayInMilliseconds);

            SHDocVw.WebBrowser ab = ((SHDocVw.WebBrowser)ie);
            HTMLDocument doc = ((HTMLDocument)ab.Document); //Get document
            IHTMLElement button = doc.getElementById("btn_download"); //Define HTML button

            button.click();
            
            string tempFile = $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.lf.html";

            await JavaScriptProcessingTime();
            doc = ((HTMLDocument)ab.Document);

            using (var output = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] buffer = ASCIIEncoding.UTF8.GetBytes(((HTMLDocument)ab.Document).documentElement.outerHTML);
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
            for (int i = (mills / 1000)-1; i > 0; i--)
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
