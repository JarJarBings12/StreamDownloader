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

namespace StreamDownloaderDownload.Hosters.Default
{
    public class StreamCloud: Hoster
    {

        #region variables and properties 
        private readonly Regex _BaseUrlPattern = new Regex(@"http://streamcloud.eu/(.*)/(.*)");
        private readonly Regex _SourceUrlPattern = new Regex("file:\\s*\\\"(.*)\\\"");
        private const int _JavaScriptProcessingTime = 15;
        
        /* Properties */
        public override Regex BaseUrlPattern => _BaseUrlPattern;
        public override Regex SourceUrlPattern => _SourceUrlPattern;

        public override bool NeedDelay => true;
        public override int DelayInMilliseconds => 11000;
        #endregion

        public sealed override async Task<string> GetSourceLink(string url)
        {
            SHDocVw.InternetExplorer a = new SHDocVw.InternetExplorer(); //Create new browser (IE)
            a.Visible = false; //TODO Remove
            a.Navigate2(url);
            
            while (a.ReadyState != tagREADYSTATE.READYSTATE_COMPLETE)
            { }
            
            await Pause(DelayInMilliseconds);

            SHDocVw.WebBrowser ab = ((SHDocVw.WebBrowser)a);
            HTMLDocument abd = ((HTMLDocument)ab.Document); //Get document
            IHTMLElement abde = abd.getElementById("btn_download"); //Define HTML button

            abde.click();
            
            string tempFile = $"{System.IO.Path.GetTempPath()}{Guid.NewGuid().ToString()}.lf.html";

            await JavaScriptProcessingTime();
            abd = ((HTMLDocument)ab.Document);

            using (var output = new FileStream(tempFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                byte[] buffer = ASCIIEncoding.UTF8.GetBytes(((HTMLDocument)ab.Document).documentElement.outerHTML);
                await output.WriteAsync(buffer, 0, buffer.Length);
            }

            var line = string.Empty;
            var input = new StreamReader(tempFile);
            Match match = null;

            while ((line = input.ReadLine()) != null)
            {
                if (line == "")
                    continue;
                match = _SourceUrlPattern.Match(line);

                if (match.Success)
                    break;
            }
            return match.Groups[1].Value;
        }

        public sealed override async Task Pause(int mills)
        {
            for (int i = (mills / 1000)-1; i > 0; i--)
            {
                UpdateStatus($"Download link will be made available in { (i/1000) } seconds.");
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
