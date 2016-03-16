using System.Windows;
using System.Drawing;
using System.Windows.Controls;
using StreamDownloaderDownload;

namespace StreamDownloaderControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:StreamDownloaderControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:StreamDownloaderControls;assembly=StreamDownloaderControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:DownloadListItem/>
    ///
    /// </summary>
    public class DownloadListItem: Control
    {

        static DownloadListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DownloadListItem), new FrameworkPropertyMetadata(typeof(DownloadListItem)));
        }

        #region properties
        public static readonly DependencyProperty FileNameProperty = DependencyProperty.RegisterAttached("FileName", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadFolderProperty = DependencyProperty.RegisterAttached("DownloadFolder", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadLinkProperty = DependencyProperty.RegisterAttached("DownloadLink", typeof(string), typeof(DownloadListItem));
        public static readonly DependencyProperty DownloadProgressProperty = DependencyProperty.RegisterAttached("DownloadProgress", typeof(double), typeof(DownloadListItem), new PropertyMetadata(0D));

        public DownloadListItem(string FileName, string DownlaodFolder, string DownloadLink, double DownloadProgress)
        {
            this.FileName = FileName;
            this.DownloadFolder = DownloadFolder;
            this.DownloadLink = DownloadLink;
            this.DownloadProgress = DownloadProgress;
        }

        #endregion

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

        public double DownloadProgress
        {
            get { return (double) base.GetValue(DownloadProgressProperty); }
            set { base.SetValue(DownloadProgressProperty, value); }
        }

        public void UpdateThumbnail(System.Windows.Controls.Image Thumbnail)
        {
            ((System.Windows.Controls.Image)GetTemplateChild("Thumbnail")).Source = Thumbnail.Source;
        }

        public void UpdateDownloadProgress(double progress)
        {
            this.DownloadProgress = progress;
        }

        public void DownloadCompleted(FileDownload download)
        {
            MessageBox.Show("DONE");
        }
    }
}