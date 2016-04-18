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
using StreamDownloaderDownload.Download;

namespace StreamDownloaderControls
{
    public class DropdownContextMenu: ContextMenu
    {
        private readonly DownloadListItem _listItem;
        private  DownloadTask _downloadTask;

        static DropdownContextMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DropdownContextMenu), new FrameworkPropertyMetadata(typeof(DropdownContextMenu)));
        }

        public DropdownContextMenu(DownloadListItem listItem)
        {
            _listItem = listItem;
        }

        public override void OnApplyTemplate()
        {
            ((Button)GetTemplateChild("tb_Pause")).Click += (sender, e) => 
            {
                var icon = ((TextBlock)GetTemplateChild("tb_Pause_Icon"));
                if (_listItem.DownloadTask.IsPaused)
                {
                    icon.Text = "&#E102";
                    _listItem.DownloadTask.Start();
                }
                else
                {
                    icon.Text = "&#E103";
                    _listItem.DownloadTask.Pause();
                }
            };

            ((Button)GetTemplateChild("tb_Save")).Click += (sender, e) =>
            {
                _listItem.DownloadTask.ContinuingLater();
            };
            base.OnApplyTemplate();
        }
    }
}
