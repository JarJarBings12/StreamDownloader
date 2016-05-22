using StreamDownloaderDownload.Download.JSON;
using StreamDownloaderDownload.FileExtensions;
using StreamDownloaderDownload.Hosts;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace StreamDownloaderControls.UserControls
{
    /// <summary>
    /// Interaction logic for NewDownlaodSettings.xaml
    /// </summary>
    public partial class DownlaodSettings: Window
    {
        #region C# Variables and properties

        public BitmapImage HostIcon
        {
            get { return (BitmapImage)base.GetValue(HostIconProperty); }
            set { base.SetValue(HostIconProperty, value); }
        }

        public string HostName
        {
            get { return ((HostListItem)base.GetValue(SelectedHostProperty)).Name; }
            set { base.SetValue(HostNameProperty, value); }
        }

        public string DownloadURL
        {
            get { return (string)base.GetValue(DownlaodUrlProperty); }
            set { base.SetValue(DownlaodUrlProperty, value); }
        }

        public string DownloadFolder
        {
            get { return (string)base.GetValue(DownloadFolderProperty); }
            set { base.SetValue(DownloadFolderProperty, value); }
        }

        public string Filename
        {
            get { return (string)base.GetValue(FilenameProperty); }
            set { base.SetValue(FilenameProperty, value); }
        }

        public IEnumerable<FileExtensionListItem> FileExtensions
        {
            get { return (IEnumerable<FileExtensionListItem>)base.GetValue(FileExtensionsProperty); }
            set { base.SetValue(FileExtensionsProperty, value); }
        }

        public FileExtension SelectedFileExtension
        {
            get { return ((FileExtensionListItem)base.GetValue(SelectedFileExtensionsProperty)).FileExtension; }
            set { base.SetValue(SelectedFileExtensionsProperty, new FileExtensionListItem(value.Extension, value)); }
        }

        public UserControl AdditionalSettigns
        {
            get { return (UserControl)base.GetValue(AdditionalSettingsProperty); }
            set
            {
                this.Height = (value == null) ? 108 : 108 + value.Height;
                base.SetValue(AdditionalSettingsProperty, value);
            }
        }

        private DownloadData _downloadData
        {
            get
            {
                var dd = new DownloadData()
                {
                    FileHost = _host,
                    RawURL = DownloadURL,
                    SourceURL = string.Empty,
                    DownloadDestination = DownloadFolder,
                    TempFileDestination = string.Empty,
                    FileName = Filename,
                    FullFileName = $"{Filename}{((_cancel) ? "" : SelectedFileExtension.Extension)}",
                    FileExtension = (_cancel) ? null : SelectedFileExtension.Extension,
                    TempHostSettings = new Hashtable()
                };

                dd.TempHostSettings.Add("cancel", _cancel);

                return dd;
            }
        }

        private Host _host;
        private volatile bool _release = false;
        private volatile bool _cancel = false;

        #endregion C# Variables and properties

        #region WPF Properties

        /// <summary>
        /// Host icon property
        /// </summary>
        public static readonly DependencyProperty HostIconProperty = DependencyProperty.RegisterAttached("HostIcon", typeof(BitmapImage), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Host name property
        /// </summary>
        public static readonly DependencyProperty HostNameProperty = DependencyProperty.RegisterAttached("HostName", typeof(string), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Download URL property
        /// </summary>
        public static readonly DependencyProperty DownlaodUrlProperty = DependencyProperty.RegisterAttached("DownloadURL", typeof(string), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Download folder property
        /// </summary>
        public static readonly DependencyProperty DownloadFolderProperty = DependencyProperty.RegisterAttached("DownloadFolder", typeof(string), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Filename property
        /// </summary>
        public static readonly DependencyProperty FilenameProperty = DependencyProperty.RegisterAttached("Filename", typeof(string), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Selected host property
        /// </summary>
        public static readonly DependencyProperty SelectedHostProperty = DependencyProperty.RegisterAttached("SelectedHost", typeof(HostListItem), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Supported file extension property
        /// </summary>
        public static readonly DependencyProperty FileExtensionsProperty = DependencyProperty.RegisterAttached("FileExtensions", typeof(IEnumerable), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Selected file extension property
        /// </summary>
        public static readonly DependencyProperty SelectedFileExtensionsProperty = DependencyProperty.RegisterAttached("SelectedFileExtension", typeof(FileExtensionListItem), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        /// <summary>
        /// Additional settings page property
        /// </summary>
        public static readonly DependencyProperty AdditionalSettingsProperty = DependencyProperty.RegisterAttached("AdditionalSettings", typeof(UserControl), typeof(DownlaodSettings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        #endregion WPF Properties

        #region Constructors

        public DownlaodSettings()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Public functions

        /// <summary>
        /// Open a new download settings dialog.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<DownloadData> OpenDialog(Window owner, DownloadData data)
        {
            //Initialize window
            var temp = new DownlaodSettings()
            {
                Owner = owner,
                _host = data.FileHost,
                HostIcon = data.FileHost.HostIcon,
                HostName = data.FileHost.HostName,
                DownloadURL = data.RawURL,
                DownloadFolder = data.DownloadDestination,
                Filename = data.FileName,
                AdditionalSettigns = (data.FileHost.CustomSettings == null) ? null : new DefaultSettings()
            };

            //Load file extensions
            var sfe = new List<FileExtensionListItem>();
            if (data.FileHost.SupportedFileExtensions != null)
                foreach (var extension in data.FileHost.SupportedFileExtensions)
                    sfe.Add(new FileExtensionListItem(extension.Extension, extension));

            temp.FileExtensions = sfe; //Apply file extensions
            temp.Show();

            temp.Owner.IsEnabled = false; //Lock main window
            await WaitToContinue(temp); //Await Submit or cancel click
            temp.Close();
            temp.Owner.IsEnabled = true; //Unlock main window

            return temp._downloadData; //return result
        }

        #endregion Public functions

        #region Private functions

        private static async Task WaitToContinue(DownlaodSettings downloadSettings)
        {
            while (!downloadSettings._release)
                await Task.Delay(100);
        }

        #region GUI Interaction logic

        private void OpenFolderBrowser(object sender, MouseButtonEventArgs e)
        {
            using (var FolderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DownloadFolder = FolderDialog.SelectedPath;
                }
            }
        }

        private void submit(object sender, RoutedEventArgs e)
        {
            _release = true;
        }

        private void cancel(object sender, RoutedEventArgs e)
        {
            _cancel = true;
            _release = true;
        }

        #region Mouse wheel scroll

        /// <summary>
        /// Handel the mouse wheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandelMouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            double offset = textBox.HorizontalOffset + (e.Delta / 12);
            offset = (offset < 0) ? 0 : offset;
            textBox.ScrollToHorizontalOffset(offset);
        }

        #endregion Mouse wheel scroll

        #endregion GUI Interaction logic

        #endregion Private functions
    }
}