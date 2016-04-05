using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using StreamDownloaderDownload.Download;
using System.Windows.Threading;

namespace StreamDownloaderControls
{
    public class DownloadListItem: Control
    {
        #region C# Variables and properties
        public string FileName
        {
            get { return (string)$"File name: { base.GetValue(FileNameProperty) }"; }
            set { base.SetValue(FileNameProperty, $"File name: {value}"); }
        }

        public string DownloadFolder
        {
            get { return (string)$"Download folder: { base.GetValue(DownloadFolderProperty) }"; }
            set { base.SetValue(DownloadFolderProperty, $"Download folder: {value}"); }
        }

        public string DownloadLink
        {
            get { return (string)$"Download link: { base.GetValue(DownloadLinkProperty) }"; }
            set { base.SetValue(DownloadLinkProperty, $"Download link: {value}"); }
        }

        public string Downloaded
        {
            get { return (string)$"Downloaded: {base.GetValue(DownloadedProperty)}"; }
            set { base.SetValue(DownloadedProperty, $"Downloaded: {value}"); }
        }

        public string Status
        {
            get { return (string)$"Status: {base.GetValue(DownloadStatusProperty)}"; }
            set { base.SetValue(DownloadStatusProperty, $"Status: {value}"); }
        }

        public double DownloadProgress
        {
            get { return (double)base.GetValue(DownloadProgressProperty); }
            set { base.SetValue(DownloadProgressProperty, value); }
        }

        private ulong _FullContentLengthInBytes; // 1 Byte = 8 Bit's
        private double _FullContentLengthInKilobytes; // 1 Kilobyte = 1024 Byte's
        private double _FullContentLengthInMegabytes; // 1 Megabyte = 1024 Kilobyte's = 1048576 Byte's
        private double _FullContentLengthInGigabytes; // 1 Gigabyte = 1024 Megabyte's = 1073741824 Byte's
        #endregion

        #region WPF properties
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.RegisterAttached("FileName", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadFolderProperty = DependencyProperty.RegisterAttached("DownloadFolder", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadLinkProperty = DependencyProperty.RegisterAttached("DownloadLink", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadedProperty = DependencyProperty.RegisterAttached("Downloaded", typeof(string), typeof(DownloadListItem), new PropertyMetadata("Downloaded: N/A"));
        public static readonly DependencyProperty DownloadStatusProperty = DependencyProperty.RegisterAttached("Status", typeof(string), typeof(DownloadListItem), new PropertyMetadata("Status: N/A"));
        public static readonly DependencyProperty DownloadProgressProperty = DependencyProperty.RegisterAttached("DownloadProgress", typeof(double), typeof(DownloadListItem), new PropertyMetadata(0D));
        #endregion

        #region Constructors
        static DownloadListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DownloadListItem), new FrameworkPropertyMetadata(typeof(DownloadListItem)));
        }

        public DownloadListItem(string fileName, string downloadFolder, string downloadLink, double downloadProgress)
        {
            FileName = fileName;
            DownloadFolder = downloadFolder;
            DownloadLink = downloadLink;
            DownloadProgress = downloadProgress;
        }
        #endregion

        public void CalculateFullContentLengths(ulong FullContentLengthInBytes)
        {
            _FullContentLengthInBytes = FullContentLengthInBytes;
            _FullContentLengthInKilobytes = (FullContentLengthInBytes / 1024);
            _FullContentLengthInMegabytes = (FullContentLengthInBytes / 1048576);
            _FullContentLengthInGigabytes = (FullContentLengthInBytes / 1073741824);
        }

        /// <summary>
        /// Constitutes the miniature view.
        /// </summary>
        /// <param name="Thumbnail">Thumbnail image</param>
        public void UpdateThumbnail(System.Windows.Controls.Image Thumbnail)
        {
            ((System.Windows.Controls.Image)GetTemplateChild("Thumbnail")).Source = Thumbnail.Source;
        }

        /// <summary>
        /// Calculate and display the current downloaded bytes in kilobyte's, megabyte's and gigabyte's.
        /// </summary>
        /// <param name="bytes">Downloaded / written bytes.</param>
        public void UpdateDownloaded(ulong bytes)
        {
            if (bytes < 1048576) //1 KB = 1024 Byte's 
                Downloaded = $"{(bytes / 1024)} / {_FullContentLengthInKilobytes} KB";
            else if (bytes < 1073741824) // 1 MB = 1024 KB = 1048576 Bytes's
                Downloaded = $"{(bytes / 1048576)} / { _FullContentLengthInMegabytes} MB";
            else if (bytes > 1073741824) // 1 GB = 1024 MB = 1073741824
                Downloaded = $"{(bytes / 1073741824)} / {_FullContentLengthInGigabytes} GB";
            else
                Downloaded = $"{(bytes)} B";
        }

        /// <summary>
        /// Updates the status message on the UI.
        /// </summary>
        /// <param name="status">Current status message.</param>
        public void UpdateDownloadStatus(string status)
        {
            this.Status = status;
        }

        /// <summary>
        /// Updates the download progress on the UI
        /// </summary>
        /// <param name="progress">The current progress in percent</param>
        public void UpdateDownloadProgress(double progress)
        {
            this.DownloadProgress = progress;
        }

        #region Events    
        /// <summary>
        /// Download status event delegate.
        /// </summary>
        /// <param name="sender">Trigger</param>
        /// <param name="e">Event arguments</param>
        public void DownloadStatusChanged(object sender, DownloadStatusChangedEventArgs e)
        {
            UpdateDownloadStatus(e.Message);
            switch (e.Status)
            {
                case DownloadStatus.DOWNLOADING:
                    break;
                case DownloadStatus.COMPLETED:
                    break;
                case DownloadStatus.PAUSED:
                    break;
                case DownloadStatus.CONTINUNING_LATER:
                    break;
                case DownloadStatus.ERROR:
                    break;
            }
        }

        /// <summary>
        /// Progress changed event delegate.
        /// </summary>
        /// <param name="sender">Trigger</param>
        /// <param name="e">Event arguments</param>
        public void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            //Invoke the asynchronous call, synchronous into the UI thread.
            Dispatcher.Invoke(() => 
            {
                UpdateDownloadProgress(e.Progress);
                UpdateDownloaded(e.WrittenBytes);
            });
        }

        #endregion

        public void DownloadCompleted(FileDownload download)
        {
            MessageBox.Show("DONE");
        }
    }
}