using StreamDownloaderControls;
using StreamDownloaderControls.UserControls;
using StreamDownloaderDownload.Download.JSON;
using StreamDownloaderDownload.FileExtensions.Default;
using StreamDownloaderDownload.Hosts;
using StreamDownloaderDownload.Hosts.Default;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StreamDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: FlatWindow
    {
        private List<HostListItem> _hosts = new List<HostListItem>();
        private List<FileExtensionListItem> _fileExtensions = new List<FileExtensionListItem>();
        private readonly Brush _placeholderGray = new SolidColorBrush(Color.FromRgb(209, 209, 209));
        private readonly Brush _fontcolorBlack = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private HashSet<Task> _activeDownloads = new HashSet<Task>();

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Download button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void DownloadSubmit_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender; //Cast sender to button
            button.IsEnabled = false; //Disable download button

            //Initialize download

            /** Host select **/
            var response = await HostSelect.OpenSelect(listBox, _hosts); //Show list select item and store result in "response"
            button.IsEnabled = true; //Enable download button again.
            listBox.Items.RemoveAt(0); //Removing list item from list.

            //Return if cancelled
            if ((bool)response.TempHostSettings["cancel"])
                return;

            /** Download settings **/
            response.DownloadDestination = Properties.Settings.Default.DownloadFolder; //Set default download folder
            response = await DownlaodSettings.OpenDialog(this, response); //Open download settings

            //Return if cancelled
            if ((bool)response.TempHostSettings["cancel"])
                return;

            //Set temp folder
            response.TempFileDestination = (Properties.Settings.Default.CustomTempDownloadFolder) ? Properties.Settings.Default.TempDownloadFolder : Properties.Settings.Default.DEFAULT_TempDownloadFolder;

            /** Prepare download **/

            var listItem = new StreamDownloaderControls.UserControls.DownloadListItem()
            {
                Filename = response.FileName,
                DownloadFolder = response.DownloadDestination,
                DownloadURL = response.RawURL
            };

            listBox.Items.Add(listItem);

            var downloadContainer = new DownloadContainer(listItem, response);

            await downloadContainer.Initialize();
            downloadContainer.Start();
        }

        private async void ReloadDownload_Click(object sender, RoutedEventArgs e)
        {
            using (var FolderDialog = new System.Windows.Forms.OpenFileDialog())
            {
                FolderDialog.InitialDirectory = (Properties.Settings.Default.CustomTempDownloadFolder) ? Properties.Settings.Default.DownloadFolder : Properties.Settings.Default.DEFAULT_TempDownloadFolder;
                FolderDialog.Filter = "Pdi files (*.dtemp.pdi;*.pdi)|*.dtemp.pdi;*.pdi";
                FolderDialog.Multiselect = true;

                var result = FolderDialog.ShowDialog();

                if (result != System.Windows.Forms.DialogResult.OK)
                    return;

                foreach (String file in FolderDialog.FileNames)
                {
                    var temp = DownloadData.DeserializeDownloadData(file);
                    /** Prepare download **/
                    var listItem = new StreamDownloaderControls.UserControls.DownloadListItem()
                    {
                        Filename = temp.FileName,
                        DownloadFolder = temp.DownloadDestination,
                        DownloadURL = temp.RawURL
                    };

                    listBox.Items.Add(listItem);

                    var downloadContainer = new DownloadContainer(listItem, temp);

                    /** Start **/
                    await downloadContainer.Initialize();
                    downloadContainer.Start();
                }
            }
        }

        public override void OnApplyTemplate()
        {
            Icon = BitmapToBitmapImage(Properties.Resources.icon);
            ((Button)GetTemplateChild("DownloadButton")).Click += DownloadSubmit_Click;
            ((Button)GetTemplateChild("ReloadDownload")).Click += ReloadDownload_Click;
            ((Button)GetTemplateChild("SettingsButton")).Click += (sender, e) => { new Settings().ShowDialog(); };
            RegisterHost("Vivo", typeof(Vivo));
            RegisterHost("StreamCloud", typeof(StreamCloud));
            RegisterHost("PowerWatch", typeof(PowerWatch));
            _fileExtensions.Add(new FileExtensionListItem("mp4", new MP4()));
            base.OnApplyTemplate();
        }

        public override void EndInit()
        {
            if (Properties.Settings.Default.FIRST_RUN)
                Settings.Initialize();
            base.EndInit();
        }

        public Host GetHost(string link)
        {
            foreach (HostListItem h in _hosts)
            {
                Host host = ((Host)Activator.CreateInstance(h.FileHost));
                if (host.BaseUrlPattern.IsMatch(link))
                    return host;
            }
            return null;
        }

        /// <summary>
        /// Register a new host for the <seealso cref="HostSelect"/> dialog. />
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="host"></param>
        protected void RegisterHost(string displayName, Type host)
        {
            var temp = (Host)Activator.CreateInstance(host);
            _hosts.Add(new HostListItem()
            {
                DisplayName = displayName,
                HostIcon = temp.HostIcon,
                FileHost = host
            });
        }

        public static System.Windows.Media.Imaging.BitmapImage BitmapToBitmapImage(System.Drawing.Bitmap bitmap)
        {
            using (var ms = new System.IO.MemoryStream())
            {
                //Save bitmap into the memory stream and set stream offset to 0
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                //Create new bitmap image set the memory stream as source and return the bitmap.
                System.Windows.Media.Imaging.BitmapImage bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}