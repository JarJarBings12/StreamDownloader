using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.IO;

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

        private Regex _OnlyNumeric = new Regex("[0-9]+");
        #endregion

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
        #endregion

        public Settings()
        {
            InitializeComponent();
        }

        public override void OnApplyTemplate()
        {
            /* Configure download folder */
            if (Properties.Settings.Default.DownloadFolder.Equals("N/A"))
            {
                var path = string.Empty;
                GetDownlaodFolder(out path);
                Properties.Settings.Default.DownloadFolder = path;
            }

            if (Properties.Settings.Default.TempDownloadFolder.Equals("N/A"))
            {
                var path = string.Empty;
                GetDownlaodFolder(out path);
                Properties.Settings.Default.TempDownloadFolder = path;
            }

            /* Load settings */
            DownloadFolder = Properties.Settings.Default.DownloadFolder;
            CustomTempDownloadFolder = Properties.Settings.Default.CustomTempDownloadFolder;
            TempDownloadFolder = Properties.Settings.Default.TempDownloadFolder;
            CustomDownloadBufferSize = Properties.Settings.Default.CustomDownloadBufferSize;
            DownloadBufferSize = (Properties.Settings.Default.DownloadBufferSize / 1024);

            /* Apply click events to the reset buttons */
            b_ResetDownloadFolder.Click += (sender, e) => { var path = string.Empty; GetDownlaodFolder(out path); DownloadFolder = path; };
            b_ResetCustomTempFolder.Click += (sender, e) => { CustomTempDownloadFolder = Properties.Settings.Default.DEFAULT_CustomTempDownloadFolder; TempDownloadFolder = AppDomain.CurrentDomain.BaseDirectory; };
            b_ResetCustomBufferSize.Click += (sender, e) => { CustomDownloadBufferSize = Properties.Settings.Default.DEFAULT_CustomDownloadBufferSize; DownloadBufferSize = (Properties.Settings.Default.DEFAULT_DownloadBufferSize / 1024); };

            base.OnApplyTemplate();
        }

        protected void OpenFolderBrowser(out string path)
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

        #region Events
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

        protected void Close(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected void DownloadFolderFocus(object sender, RoutedEventArgs e)
        {
            var path = string.Empty;
            OpenFolderBrowser(out path);
            DownloadFolder = path;
        }

        protected void TempDownloadFolderFocus(object sender, RoutedEventArgs e)
        {
            var path = string.Empty;
            OpenFolderBrowser(out path);
            TempDownloadFolder = path;
        }

        protected void BufferSizeTextChanged(object sender, TextChangedEventArgs e)
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

        /// <summary>
        /// Handel the mouse wheel 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void HandelMouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox textBox = ((TextBox)sender);
            double offset = textBox.HorizontalOffset + (e.Delta / 12);
            offset = (offset < 0) ? 0 : offset;
            textBox.ScrollToHorizontalOffset(offset);
        }
        #endregion

        protected internal static void Initialize()
        {
            var path = string.Empty;
            GetDownlaodFolder(out path);
            Properties.Settings.Default.DownloadFolder = path;
            Properties.Settings.Default.TempDownloadFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TEMP");
            Properties.Settings.Default.Save();

            Directory.CreateDirectory(Properties.Settings.Default.TempDownloadFolder);
        }

        #region C Interop
        [DllImport("Shell32.dll")]
        protected static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        protected static void GetDownlaodFolder(out string path)
        {
            IntPtr outPath;
            SHGetKnownFolderPath(new Guid("{374DE290-123F-4565-9164-39C4925E467B}"), (uint)0x00004000, new IntPtr(0), out outPath);
            path = Marshal.PtrToStringUni(outPath);
        }
        #endregion
    }
}
