using System;
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
using StreamDownloaderDownload.FileExtensions;

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

        #endregion

        #region WPF properties
        public static readonly DependencyProperty HosterNameProperty = DependencyProperty.RegisterAttached("DisplayName", typeof(string), typeof(FileExtensionListItem));
        #endregion

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
        #endregion
    }
}
