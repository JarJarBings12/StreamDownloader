using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace StreamDownloader
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings: Window
    {
        #region Variables and properties

        public string DownloadFolder
        {
            get { return (string)base.GetValue(DownloadFolderProperty); }
            set { base.SetValue(DownloadFolderProperty, value); }
        }

        public bool CustomTempDownloadFolder
        {
            get { return (bool)base.GetValue(CustomTempDownloadFolderProperty); }
            set { base.SetValue(CustomTempDownloadFolderProperty, value); }
        }

        public string TempDownloadFolder
        {
            get { return (string)base.GetValue(TempDownloadFolderProperty); }
            set { base.SetValue(TempDownloadFolderProperty, value); }
        }

        public bool CustomDownloadBufferSize
        {
            get { return (bool)base.GetValue(CustomDownloadBufferSizeProperty); }
            set { base.SetValue(CustomDownloadBufferSizeProperty, value); }
        }

        public uint DownloadBufferSize
        {
            get { return (uint)base.GetValue(DownloadBufferSizeProperty); }
            set { base.SetValue(DownloadBufferSizeProperty, value); }
        }

        private Regex _onlyNumeric = new Regex("[0-9]+");

        #endregion Variables and properties

        #region WPF properties

        public static readonly DependencyProperty DownloadFolderProperty = DependencyProperty.RegisterAttached("DownloadFolder", typeof(string), typeof(Settings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty CustomTempDownloadFolderProperty = DependencyProperty.RegisterAttached("CustomTempDownloadFolder", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata
        {
            DefaultValue = false,
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty TempDownloadFolderProperty = DependencyProperty.RegisterAttached("TempDownloadFolder", typeof(string), typeof(Settings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty CustomDownloadBufferSizeProperty = DependencyProperty.RegisterAttached("CustomDownloadBufferSize", typeof(bool), typeof(Settings), new FrameworkPropertyMetadata
        {
            DefaultValue = false,
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty DownloadBufferSizeProperty = DependencyProperty.RegisterAttached("DownloadBufferSize", typeof(uint), typeof(Settings), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });

        #endregion WPF properties

        public Settings()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            /* Configure download folder */
            if (Properties.Settings.Default.DownloadFolder.Equals("N/A"))
                Properties.Settings.Default.DownloadFolder = Properties.Settings.Default.DEFAULT_DownloadFolder;

            if (Properties.Settings.Default.TempDownloadFolder.Equals("N/A"))
                Properties.Settings.Default.TempDownloadFolder = Properties.Settings.Default.DEFAULT_TempDownloadFolder;

            /* Load settings */
            DownloadFolder = Properties.Settings.Default.DownloadFolder;
            CustomTempDownloadFolder = Properties.Settings.Default.CustomTempDownloadFolder;
            TempDownloadFolder = Properties.Settings.Default.TempDownloadFolder;
            CustomDownloadBufferSize = Properties.Settings.Default.CustomDownloadBufferSize;
            DownloadBufferSize = (Properties.Settings.Default.DownloadBufferSize / 1024);

            /* Apply click events to the reset buttons */
            b_ResetDownloadFolder.Click += (sender, e) => { DownloadFolder = Properties.Settings.Default.DEFAULT_DownloadFolder; };
            b_ResetCustomTempFolder.Click += (sender, e) => { CustomTempDownloadFolder = Properties.Settings.Default.DEFAULT_CustomTempDownloadFolder; TempDownloadFolder = Properties.Settings.Default.DEFAULT_TempDownloadFolder; };
            b_ResetCustomBufferSize.Click += (sender, e) => { CustomDownloadBufferSize = Properties.Settings.Default.DEFAULT_CustomDownloadBufferSize; DownloadBufferSize = (Properties.Settings.Default.DEFAULT_DownloadBufferSize / 1024); };

            base.OnApplyTemplate();
        }

        #region Private functions

        private void OpenFolderBrowser(out string path)
        {
            using (var FolderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    path = FolderDialog.SelectedPath + @"\";
                    return;
                }
            }
            GetDownlaodFolder(out path);
            return;
        }

        #region GUI interaction logic

        private void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Open download folder dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadFolderFocus(object sender, RoutedEventArgs e)
        {
            var path = string.Empty;
            OpenFolderBrowser(out path);
            DownloadFolder = path;
        }

        /// <summary>
        /// Open temp download folder dialog.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TempDownloadFolderFocus(object sender, RoutedEventArgs e)
        {
            var path = string.Empty;
            OpenFolderBrowser(out path);
            TempDownloadFolder = path;
        }

        /// <summary>
        /// Handel the mouse wheel.
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

        private void BufferSizeTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = ((TextBox)sender);

            var value = 0;

            if ((int.TryParse(textBox.Text, out value)) && value <= 64)
                return;
            else if (!int.TryParse(textBox.Text, out value))
                e.Handled = true;
            else if (value > 64)
                DownloadBufferSize = 64;
            else
                return;
        }

        #endregion GUI interaction logic

        #endregion Private functions

        #region Protected functions

        protected internal static void Initialize()
        {
            var path = string.Empty;
            GetDownlaodFolder(out path);
            Properties.Settings.Default.DownloadFolder = path;
            Properties.Settings.Default.DEFAULT_DownloadFolder = path;

            path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Temp");
            Properties.Settings.Default.DEFAULT_TempDownloadFolder = path;
            Properties.Settings.Default.TempDownloadFolder = path;
            Properties.Settings.Default.FIRST_RUN = false;
            Properties.Settings.Default.Save();

            Directory.CreateDirectory(Properties.Settings.Default.TempDownloadFolder);
            Directory.CreateDirectory($"{Properties.Settings.Default.TempDownloadFolder}\\SAVE");
        }

        protected void ApplySettings(object sender, RoutedEventArgs e)
        {
            /* Apply settings to configuration */
            Properties.Settings.Default.DownloadFolder = DownloadFolder;
            Properties.Settings.Default.CustomTempDownloadFolder = CustomTempDownloadFolder;
            Properties.Settings.Default.TempDownloadFolder = TempDownloadFolder;
            Properties.Settings.Default.CustomDownloadBufferSize = CustomDownloadBufferSize;
            Properties.Settings.Default.DownloadBufferSize = (DownloadBufferSize * 1024);
            Properties.Settings.Default.Save();
            this.Close();
        }

        #region C Interops

        [DllImport("Shell32.dll")]
        protected static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        protected static void GetDownlaodFolder(out string path)
        {
            IntPtr outPath;
            SHGetKnownFolderPath(new Guid("{374DE290-123F-4565-9164-39C4925E467B}"), (uint)0x00004000, new IntPtr(0), out outPath);
            path = Marshal.PtrToStringUni(outPath);
        }

        #endregion C Interops

        #endregion Protected functions
    }
}