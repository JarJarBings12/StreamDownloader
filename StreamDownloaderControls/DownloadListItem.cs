using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using StreamDownloaderDownload.Download;

namespace StreamDownloaderControls
{
    public class DownloadListItem: Control
    {

        static DownloadListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DownloadListItem), new FrameworkPropertyMetadata(typeof(DownloadListItem)));
        }

        #region WPF properties
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.RegisterAttached("FileName", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadFolderProperty = DependencyProperty.RegisterAttached("DownloadFolder", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadLinkProperty = DependencyProperty.RegisterAttached("DownloadLink", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadMaximumProperty = DependencyProperty.RegisterAttached("DownloadMaximum", typeof(double), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadProgressProperty = DependencyProperty.RegisterAttached("DownloadProgress", typeof(double), typeof(DownloadListItem), new PropertyMetadata(0D));

        public DownloadListItem(string FileName, string DownlaodFolder, string DownloadLink, int DownloadMaximum, double DownloadProgress)
        {
            this.FileName = FileName;
            this.DownloadFolder = DownloadFolder;
            this.DownloadLink = DownloadLink;
            this.DownloadMaximum = DownloadMaximum;
            this.DownloadProgress = DownloadProgress;
        }

        #endregion

        #region Properties
        public string FileName
        {
            get { return (string) $"File name: { base.GetValue(FileNameProperty) }"; }
            set { base.SetValue(FileNameProperty, value); }
        }

        public string DownloadFolder
        {
            get { return (string) $"Download folder: { base.GetValue(DownloadFolderProperty) }"; }
            set { base.SetValue(DownloadFolderProperty, value); }
        }

        public string DownloadLink
        {
            get { return (string) $"Download link: { base.GetValue(DownloadLinkProperty) }"; }
            set { base.SetValue(DownloadLinkProperty, value); }
        }

        public double DownloadMaximum
        {
            get { return (double)base.GetValue(DownloadMaximumProperty); }
            set { base.SetValue(DownloadMaximumProperty, value); }
        }

        public double DownloadProgress
        {
            get { return (double) base.GetValue(DownloadProgressProperty); }
            set { base.SetValue(DownloadProgressProperty, value); }
        }
        #endregion


        public void UpdateThumbnail(System.Windows.Controls.Image Thumbnail)
        {
            ((System.Windows.Controls.Image)GetTemplateChild("Thumbnail")).Source = Thumbnail.Source;
        }

        public void UpdateDownloadProgress(double progress)
        {
            this.DownloadProgress = progress;
        }

        #region Events    
        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadProgress = e.Progress;
        }

        #endregion

        public void DownloadCompleted(FileDownload download)
        {
            MessageBox.Show("DONE");
        }
    }
}