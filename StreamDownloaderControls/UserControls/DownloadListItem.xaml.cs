using StreamDownloaderDownload.Download;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamDownloaderControls.UserControls
{
    public delegate void DownloadStartHandler(object sneder);

    public delegate void DownloadPauseHandler(object sender);

    public delegate void DownloadSaveHandler(object sender);

    public delegate void DownloadCancelHandler(object sender);

    /// <summary>
    /// Interaction logic for UserControl2.xaml
    /// </summary>
    public partial class DownloadListItem: UserControl
    {
        public DownloadListItem()
        {
            InitializeComponent();
        }

        #region C# Variables and properties

        public BitmapImage Thumbnail
        {
            get { return (BitmapImage)base.GetValue(ThumbnailProperty); }
            set { base.SetValue(ThumbnailProperty, value); }
        }

        public string Filename
        {
            get { return (string)base.GetValue(FilenameProperty); }
            set { base.SetValue(FilenameProperty, value); }
        }

        public string DownloadFolder
        {
            get { return (string)base.GetValue(DownloadFolderProperty); }
            set { base.SetValue(DownloadFolderProperty, value); }
        }

        public string DownloadURL
        {
            get { return (string)base.GetValue(DownloadURLProperty); }
            set { base.SetValue(DownloadURLProperty, value); }
        }

        public string Status
        {
            get { return (string)base.GetValue(DownloadStatusProperty); }
            set { base.SetValue(DownloadStatusProperty, value); }
        }

        public string Downloaded
        {
            get { return (string)base.GetValue(DownloadedProperty); }
            set { base.SetValue(DownloadedProperty, value); }
        }

        public double DownloadProgress
        {
            get { return (double)base.GetValue(DownloadProgressProperty); }
            set { base.SetValue(DownloadProgressProperty, value); }
        }

        public bool IsPaused
        {
            get { return (bool)base.GetValue(DownloadStateProperty); }
            set { base.SetValue(DownloadStateProperty, value); }
        }

        public DownloadTask DownloadTask => _downloadTask;

        private DownloadTask _downloadTask;
        private ulong _fullContentLengthInBytes; // 1 Byte = 8 Bit's
        private double _fullContentLengthInKilobytes; // 1 Kilobyte = 1024 Byte's
        private double _fullContentLengthInMegabytes; // 1 Megabyte = 1024 Kilobyte's = 1048576 Byte's
        private double _fullContentLengthInGigabytes; // 1 Gigabyte = 1024 Megabyte's = 1073741824 Byte's

        #endregion C# Variables and properties

        #region WPF properties

        //Thumbnail
        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.RegisterAttached("Thumbnail", typeof(BitmapImage), typeof(DownloadListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultValue = null
        });

        //Filename
        public static readonly DependencyProperty FilenameProperty = DependencyProperty.RegisterAttached("Filename", typeof(string), typeof(DownloadListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultValue = "N/A"
        });

        //Download folder
        public static readonly DependencyProperty DownloadFolderProperty = DependencyProperty.RegisterAttached("DownloadFolder", typeof(string), typeof(DownloadListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultValue = "N/A"
        });

        //Download URL
        public static readonly DependencyProperty DownloadURLProperty = DependencyProperty.RegisterAttached("DownloadURL", typeof(string), typeof(DownloadListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultValue = "N/A"
        });

        //Status message
        public static readonly DependencyProperty DownloadStatusProperty = DependencyProperty.RegisterAttached("Status", typeof(string), typeof(DownloadListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultValue = "N/A"
        });

        //Downloaded
        public static readonly DependencyProperty DownloadedProperty = DependencyProperty.RegisterAttached("Downloaded", typeof(string), typeof(DownloadListItem), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true,
            DefaultValue = "N/A"
        });

        //Download progress
        public static readonly DependencyProperty DownloadProgressProperty = DependencyProperty.RegisterAttached("DownloadProgress", typeof(double), typeof(DownloadListItem), new FrameworkPropertyMetadata()
        {
            BindsTwoWayByDefault = true,
            DefaultValue = 0D
        });

        //Download State
        public static readonly DependencyProperty DownloadStateProperty = DependencyProperty.RegisterAttached("IsPaused", typeof(bool), typeof(DownloadListItem), new FrameworkPropertyMetadata()
        {
            BindsTwoWayByDefault = true,
            DefaultValue = true
        });

        #endregion WPF properties

        #region Public functions

        #region Event delegates

        /// <summary>
        /// Download status event delegate.
        /// </summary>
        /// <param name="sender">Trigger</param>
        /// <param name="e">Event arguments</param>
        public void DownloadStatusChanged(object sender, DownloadStatusChangedEventArgs e)
        {
            switch (e.Status)
            {
                case DownloadStatus.NOT_INITIALIZED:
                    this.menu.IsEnabled = false;
                    break;

                case DownloadStatus.INITIALIZE:
                    _downloadTask = (DownloadTask)sender;
                    CalculateContentLength(_downloadTask.Download.ContentLength.Value);
                    this.menu.IsEnabled = false;
                    break;

                case DownloadStatus.DOWNLOADING:
                    this.menu.IsEnabled = true;
                    break;

                case DownloadStatus.COMPLETED:
                    this.menu.IsEnabled = false;
                    break;

                case DownloadStatus.PAUSED:
                    break;

                case DownloadStatus.CONTINUNING_LATER:
                    this.menu.IsEnabled = false;
                    break;

                case DownloadStatus.ERROR:
                    this.menu.IsEnabled = false;
                    break;
            }
            UpdateDownloadStatus(e.Message);
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

        #endregion Event delegates

        #endregion Public functions

        #region Private functions

        /// <summary>
        /// Calculates the download length in Bytes, kilobytes, megabytes and gigabytes.
        /// </summary>
        /// <param name="FullContentLengthInBytes"></param>
        private void CalculateContentLength(ulong contentLengthInBytes)
        {
            _fullContentLengthInBytes = contentLengthInBytes;
            _fullContentLengthInKilobytes = (contentLengthInBytes / 1024);
            _fullContentLengthInMegabytes = (contentLengthInBytes / 1048576);
            _fullContentLengthInGigabytes = (contentLengthInBytes / 1073741824);
        }

        #region GUI Interaction logic

        private void Pause(object sender, RoutedEventArgs e)
        {
            if (_downloadTask.IsPaused)
                _downloadTask.Start();
            else
                _downloadTask.Pause();
            IsPaused = !IsPaused;
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            _downloadTask.ContinuingLater();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            _downloadTask.Cancel();
        }

        #endregion GUI Interaction logic

        #region GUI update functions

        /// <summary>
        /// Updates the thumbnail on the GUI.
        /// </summary>
        /// <param name="thumbnail"></param>
        private void UpdateThumbnail(BitmapImage thumbnail)
        {
            this.Thumbnail = thumbnail;
        }

        /// <summary>
        /// Updates the status message on the GUI.
        /// </summary>
        /// <param name="status">Current status message.</param>
        private void UpdateDownloadStatus(string status)
        {
            this.Status = status;
        }

        /// <summary>
        /// Calculate and display the current downloaded bytes in kilobyte's, megabyte's and gigabyte's.
        /// </summary>
        /// <param name="bytes">Downloaded / written bytes.</param>
        private void UpdateDownloaded(ulong bytes)
        {
            if (bytes < 1048576) //1 KB = 1024 Byte's
                Downloaded = $"{(bytes / 1024)} / {_fullContentLengthInKilobytes} KB";
            else if (bytes < 1073741824) // 1 MB = 1024 KB = 1048576 Bytes's
                Downloaded = $"{(bytes / 1048576)} / {_fullContentLengthInMegabytes} MB";
            else if (bytes > 1073741824) // 1 GB = 1024 MB = 1073741824
                Downloaded = $"{(bytes / 1073741824)} / {_fullContentLengthInGigabytes} GB";
            else
                Downloaded = $"{(bytes)} / {_fullContentLengthInBytes} B";
        }

        /// <summary>
        /// Updates the download progress on the GUI.
        /// </summary>
        /// <param name="progress">The current progress in percent</param>
        private void UpdateDownloadProgress(double progress)
        {
            this.DownloadProgress = progress;
        }

        #endregion GUI update functions

        #endregion Private functions
    }
}