using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using StreamDownloaderControls;
using StreamDownloaderDownload;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.Hosters;
using StreamDownloaderDownload.Hosters.Default;
using System.IO;

namespace StreamDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: FlatWindow
    {
        private StreamDownloaderDownload.StreamDownloader _streamDownloader = new StreamDownloaderDownload.StreamDownloader();
        private readonly Brush _placeholderGray = new SolidColorBrush(Color.FromRgb(209, 209, 209));
        private readonly Brush _fontcolorBlack = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// <c> GotFocus </c> event for the download link text box.
        /// Needed for the placeholder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadLink_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Equals("Download link"))
            {
                tb.Foreground = _fontcolorBlack;
                tb.Text = string.Empty;
            }
        }

        /// <summary>
        /// <c> LostFoucs </c> event for the download link text box.
        /// Needed for the placeholder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadLink_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            
            if (string.IsNullOrEmpty(tb.Text))
            {
                tb.Foreground = _placeholderGray;
                tb.Text = "Download link";
            }
        }

        private async void DownloadSubmit_Click(object sender, RoutedEventArgs e)
        {
            DownloadListItem listItem = new DownloadListItem("TEST", "mp4", "http://vivo.sx/c79861f047", 0);
            listBox.Items.Add(listItem);
            Hoster hoster = new Vivo();
            string source = await hoster.GetSourceLink("http://vivo.sx/c79861f047");
            listItem.DownloadLink = source;
            DownloadTask task = _streamDownloader.CreateDownload("TEST", "mp4", source);
            task.DownloadProgressChanged += listItem.DownloadProgressChanged;
            task.Start();
        }

        public override void OnApplyTemplate()
        {
            ((Button)GetTemplateChild("DownloadButton")).Click += DownloadSubmit_Click;
            base.OnApplyTemplate();
        }
    }
}