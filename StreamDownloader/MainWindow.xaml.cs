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
using StreamDownloaderControls;

namespace StreamDownloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: FlatWindow
    {

        private readonly Brush _PlaceholderGray = new SolidColorBrush(Color.FromRgb(209, 209, 209));
        private readonly Brush _FontcolorBlack = new SolidColorBrush(Color.FromRgb(0, 0, 0));

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// <c> GotFocus </c> event for the download link text box.
        /// Needed for the placeholder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadLink_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;

            if (tb.Text.Equals("Download link"))
            {
                tb.Foreground = _FontcolorBlack;
                tb.Text = string.Empty;
            }
        }

        /// <summary>
        /// <c> LostFoucs </c> event for the download link text box.
        /// Needed for the placeholder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadLink_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            
            if (string.IsNullOrEmpty(tb.Text))
            {
                tb.Foreground = _PlaceholderGray;
                tb.Text = "Download link";
            }
        }
    }
}
