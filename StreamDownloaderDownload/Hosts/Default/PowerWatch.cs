using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using mshtml;

namespace StreamDownloaderDownload.Hosts.Default
{
    public class PowerWatch : Host
    {
        #region variables and properties
        private Regex _BaseUrlPattern = new Regex(@"http://powerwatch.pw/(.*)");
        private Regex _SourceUrlPattern = new Regex("sources:(.*)\\[\\{file:\"(.*)\",");

        public sealed override Regex BaseUrlPattern => _BaseUrlPattern;
        public sealed override Regex SourceUrlPattern => _SourceUrlPattern;

        public sealed override bool NeedDelay => true;
        public override int DelayInMilliseconds => 6000;
        #endregion

        public sealed override async Task<string> GetSourceLink(string url)
        {
            SHDocVw.InternetExplorer ie = new SHDocVw.InternetExplorer();
            ie.Navigate2(url);

            while (ie.ReadyState != SHDocVw.tagREADYSTATE.READYSTATE_COMPLETE)
            { }

            SHDocVw.WebBrowser browser = ((SHDocVw.WebBrowser)ie);
            HTMLDocument doc = ((HTMLDocument)browser.Document);
            IHTMLElement button = doc.getElementById("btn_download");

            await Pause(8000); //Wait 8 Seconds.

            button.click(); //Click button

            string tempFile = $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.lf.html";

            await Pause(30000); //Wait 30000 Seconds.

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
                match = _SourceUrlPattern.Match(line);

                if (match.Success)
                    break;
            }
            return match.Groups[2].Value;
        }

        public sealed override async Task Pause(int interval)
        {
            await Task.Delay(interval);
        }
    }
}
