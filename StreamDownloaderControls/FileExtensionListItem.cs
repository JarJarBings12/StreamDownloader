using StreamDownloaderDownload.FileExtensions;
using System.Windows;
using System.Windows.Controls;

namespace StreamDownloaderControls
{
    public class FileExtensionListItem: Control
    {
        #region C# variables and properties

        public string DisplayName
        {
            get { return (string)base.GetValue(HosterNameProperty); }
            set { base.SetValue(HosterNameProperty, value); }
        }

        public FileExtension FileExtension { get; }

        #endregion C# variables and properties

        #region WPF properties

        public static readonly DependencyProperty HosterNameProperty = DependencyProperty.RegisterAttached("DisplayName", typeof(string), typeof(FileExtensionListItem));

        #endregion WPF properties

        #region constructors

        static FileExtensionListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FileExtensionListItem), new FrameworkPropertyMetadata(typeof(FileExtensionListItem)));
        }

        public FileExtensionListItem(string displayName, FileExtension fileExtension)
        {
            DisplayName = displayName;
            FileExtension = fileExtension;
        }

        #endregion constructors
    }
}