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
using StreamDownloaderDownload.Hosters.Default;

namespace StreamDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: FlatWindow
    {
        private DownloadTask _fileDownload;
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
            DownloadListItem downloadListItem = new DownloadListItem("TEST.mp4", @"C:/TEST.mp4", "http://4.cdn.vivo.sx:8080/get/0043082018?e=1457497518&s=250&m=video/mp4&h=I3r8K02S2XrprhpOiXkblA", 10);
            DownloadTask.UpdateDownloadProgress a = downloadListItem.UpdateDownloadProgress;
            DownloadTask.CompleteDownload b = downloadListItem.DownloadCompleted;
            StreamCloud c = new StreamCloud();
            Vivo d = new Vivo();
            string ab = await d.GetSourceLink(@"http://vivo.sx/c79861f047");
            MessageBox.Show(ab);
            _fileDownload = new DownloadTask("C:/", "TEST.mp4", ab, a, b);
            listBox.Items.Add(downloadListItem);
            _fileDownload.Start();


        }

    }
}