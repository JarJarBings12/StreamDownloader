﻿using System;
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
using StreamDownloaderDownload;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.Hosters;
using StreamDownloaderDownload.FileExtensions;
using System.Text.RegularExpressions;
using System.Collections;

namespace StreamDownloaderControls
{
    public class CreateDownload: Control
    {
        #region C# Variables and properties
        private readonly Brush _placeholderGray = new SolidColorBrush(Color.FromRgb(209, 209, 209));
        private readonly Brush _fontcolorBlack = new SolidColorBrush(Color.FromRgb(0, 0, 0));
        private const string _tb_ph_DownloadLink = "Download link";
        private const string _tb_ph_DownloadFolder = "Download folder";
        private const string _tb_ph_FileName = "File name";
        private bool _release = false;

        public IEnumerable SupportedHosters
        {
            get { return (IEnumerable)base.GetValue(SupportedHostersProperty); }
            set { base.SetValue(SupportedHostersProperty, value); }
        }

        public Hoster SelectedHoster
        {
            get { return (Hoster)base.GetValue(SelectedHosterProperty); }
            set { base.SetValue(SelectedHosterProperty, value); }
        }

        public string DownloadLink
        {
            get { return (string)base.GetValue(DownloadLinkProperty); }
            set { base.SetValue(DownloadLinkProperty, value); }
        }

        public string DownloadFolder
        {
            get { return (string)base.GetValue(DownloadFolderProperty); }
            set { base.SetValue(DownloadFolderProperty, value); }
        }

        public string FileName
        {
            get { return (string)base.GetValue(FileNameProperty); }
            set { base.SetValue(FileNameProperty, value); }
        }

        public IEnumerable FileExtensions
        {
            get { return (IEnumerable)base.GetValue(FileExtensionsProperty); }
            set { base.SetValue(FileExtensionsProperty, value); }
        }

        public FileExtension SelectedFileExtension
        {
            get { return (FileExtension)base.GetValue(SelectedFileExtensionIndexProperty); }
            set { base.SetValue(SelectedFileExtensionIndexProperty, value); }
        }
        #endregion

        #region WPF properties
        public static readonly DependencyProperty SupportedHostersProperty = DependencyProperty.RegisterAttached("SupportedHosters", typeof(IEnumerable), typeof(CreateDownload), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });
        public static readonly DependencyProperty SelectedHosterProperty = DependencyProperty.RegisterAttached("SelectedHoster", typeof(Hoster), typeof(CreateDownload), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });
        public static readonly DependencyProperty DownloadLinkProperty = DependencyProperty.RegisterAttached("DownloadLink", typeof(string), typeof(CreateDownload), new FrameworkPropertyMetadata
        {
            DefaultValue = _tb_ph_DownloadLink,           
            BindsTwoWayByDefault = true
        });
        public static readonly DependencyProperty DownloadFolderProperty = DependencyProperty.RegisterAttached("DownloadFolder", typeof(string), typeof(CreateDownload), new FrameworkPropertyMetadata
        {
            DefaultValue = _tb_ph_DownloadFolder,
            BindsTwoWayByDefault = true
        });

        public static readonly DependencyProperty FileNameProperty = DependencyProperty.RegisterAttached("FileName", typeof(string), typeof(CreateDownload), new FrameworkPropertyMetadata
        {
            DefaultValue = _tb_ph_FileName,
            BindsTwoWayByDefault = true
        });
        public static readonly DependencyProperty FileExtensionsProperty = DependencyProperty.RegisterAttached("FileExtensions", typeof(IEnumerable), typeof(CreateDownload), new FrameworkPropertyMetadata
        {
            BindsTwoWayByDefault = true
        });
        public static readonly DependencyProperty SelectedFileExtensionIndexProperty = DependencyProperty.RegisterAttached("SelectedFileExtension", typeof(FileExtension), typeof(CreateDownload));
        #endregion

        #region Constructors
        static CreateDownload()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CreateDownload), new FrameworkPropertyMetadata(typeof(CreateDownload)));
        }

        protected CreateDownload(List<Hoster> hosters, List<FileExtension> extensions)
        {
            SupportedHosters = hosters;
            FileExtensions = extensions;         
        }
        #endregion

        public static async Task<CreateDownload> ShowDialog(ContentControl MDIControl, List<Hoster> hosters, List<FileExtension> extensions)
        {
            MDIControl.Content = new CreateDownload(hosters, extensions);
            var temp = ((CreateDownload)MDIControl.Content);
            await WaitToContinue(temp);
            return temp;
        }

        public bool IsSupported(string link)
        {
            foreach (Hoster h in SupportedHosters)
            {
                if (h.BaseUrlPattern.IsMatch(link))
                    return true;
            }
            return false;
        }

        public Hoster GetHoster(string link)
        {
            foreach (Hoster h in SupportedHosters)
            {
                if (h.BaseUrlPattern.IsMatch(link))
                    return h;
            }
            return null;
        }

        public override void OnApplyTemplate()
        {
            //Download link
            var _tb_DownloadLink = ((TextBox)GetTemplateChild("tb_DownloadLink"));
            _tb_DownloadLink.Foreground = _placeholderGray;
            _tb_DownloadLink.GotFocus += (sender, e) => HandelPlaceholder(sender, _tb_ph_DownloadLink);
            _tb_DownloadLink.LostFocus += (sender, e) => HandelPlaceholder(sender, _tb_ph_DownloadLink);
            _tb_DownloadLink.TextChanged += ValidateDownloadLink;
            _tb_DownloadLink.PreviewMouseWheel += HandelMouseWheel;
            //Download folder
            var _tb_DownloadFolder = ((TextBox)GetTemplateChild("tb_DownloadFolder"));
            _tb_DownloadFolder.Foreground = _placeholderGray;
            _tb_DownloadFolder.GotFocus += (sender, e) => HandelPlaceholder(sender, _tb_ph_DownloadFolder);
            _tb_DownloadFolder.LostFocus += (sender, e) => HandelPlaceholder(sender, _tb_ph_DownloadFolder);
            _tb_DownloadFolder.PreviewMouseWheel += HandelMouseWheel;
            _tb_DownloadFolder.PreviewMouseLeftButtonDown += (sender, e) => OpenFolderBrowser();
            //File name
            var _tb_FileName = ((TextBox)GetTemplateChild("tb_FileName"));
            _tb_FileName.Foreground = _placeholderGray;
            _tb_FileName.GotFocus += (sender, e) => HandelPlaceholder(sender, _tb_ph_FileName);
            _tb_FileName.LostFocus += (sender, e) => HandelPlaceholder(sender, _tb_ph_FileName);
            _tb_FileName.PreviewMouseWheel += HandelMouseWheel;

            var _b_Cancel = ((Button)GetTemplateChild("b_cancel"));
            _b_Cancel.Click += (sender, e) => { ((ContentControl)this.Parent).Content = null; };
            var _b_Submit = ((Button)GetTemplateChild("b_submit"));
            _b_Submit.Click += (sender, e) => { _release = true; ((ContentControl)this.Parent).Content = null; };

            base.OnApplyTemplate();
        }

        protected static async Task WaitToContinue(CreateDownload dialog)
        {
            while (!dialog._release)
            {
                await Task.Delay(100);
            }
        }

        protected void ValidateDownloadLink(object sender, TextChangedEventArgs e)
        {
           // DownloadLink = ((TextBox)sender).Text;
            Hoster hoster = null;
            ComboBox hosters = GetTemplateChild("cb_Hoster") as ComboBox;
            if (string.IsNullOrEmpty(DownloadLink))
                return;
            if (IsSupported(DownloadLink))
            {
                hoster = GetHoster(DownloadLink);
                hosters.SelectedValue = hoster;
            }
            else
            {
            }
        }

        protected void OpenFolderBrowser()
        {
            using (var FolderDialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (FolderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    DownloadFolder = FolderDialog.SelectedPath;
                }
            }
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

        /// <summary>
        /// Show and hide the placeholder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="placeholder"></param>
        protected void HandelPlaceholder(object sender, string placeholder)
        {
            TextBox tb = ((TextBox)sender);
            if (!tb.Text.Equals(placeholder)) //Text not equals placeholder
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Foreground = _placeholderGray;
                    tb.Text = placeholder;
                }
                else
                {
                    //TODO Add auto select
                }
            }
            else //Text equals placeholder
            {
                tb.Foreground = _fontcolorBlack;
                tb.Text = string.Empty;
            }
        }
    }
}
