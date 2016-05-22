using StreamDownloaderDownload.Attributes;
using StreamDownloaderDownload.FileExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace StreamDownloaderDownload.Hosts
{
    public delegate void FetchStatusChangedHandler(object sender, LinkFetchStatusChangedEventArgs e);

    public abstract class Host
    {
        public event FetchStatusChangedHandler FetchStatusChanged;

        public abstract List<FileExtension> SupportedFileExtensions { get; }
        public abstract string HostName { get; }
        public abstract BitmapImage HostIcon { get; }
        public abstract Regex BaseUrlPattern { get; }
        public abstract Regex SourceUrlPattern { get; }

        public abstract int DelayInMilliseconds { get; }

        public UserControl CustomSettings { get; set; } = null;

        public abstract Task<Tuple<string, LinkFetchResult>> GetSourceLink(string url);

        /* Asynchronous pause */

        public virtual async Task Pause(int interval)
        {
            await Task.Delay(interval);
        }

        #region Public functions

        public void UpdateStatus(string NewStatus)
        {
            if (FetchStatusChanged == null)
                return;
            FetchStatusChanged(this, new LinkFetchStatusChangedEventArgs(NewStatus));
        }

        public static BitmapImage FileToBitmapImage(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            using (var ms = new MemoryStream())
            using (var bitmap = Bitmap.FromFile(filePath))
            {
                //Save bitmap into the memory stream and set stream offset to 0
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                //Create new bitmap image set the memory stream as source and return the bitmap.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (var ms = new MemoryStream())
            {
                //Save bitmap into the memory stream and set stream offset to 0
                bitmap.Save(ms, ImageFormat.Png);
                ms.Position = 0;

                //Create new bitmap image set the memory stream as source and return the bitmap.
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static Hashtable GetStaticSettingsPropertys(Page customSettings)
        {
            var settings = new Hashtable();
            FieldInfo[] fields = customSettings.GetType().GetFields();
            PropertyInfo[] properties = customSettings.GetType().GetProperties();

            var atribut = new SettingsProperty("", true);
            foreach (FieldInfo field in fields)
            {
                if (field.IsDefined(typeof(SettingsProperty)))
                {
                    atribut = (SettingsProperty)field.GetCustomAttribute(typeof(SettingsProperty), false);
                    if (!atribut.Temporary)
                        settings.Add(atribut.Key, field.GetValue(null));
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.IsDefined(typeof(SettingsProperty)))
                {
                    atribut = (SettingsProperty)property.GetCustomAttribute(typeof(SettingsProperty), false);
                    if (!atribut.Temporary)
                        settings.Add(atribut.Key, property.GetValue(null));
                }
            }

            return settings;
        }

        public static Hashtable GetTemporarySettingsProperty(Page customSettings)
        {
            var settings = new Hashtable();
            FieldInfo[] fields = customSettings.GetType().GetFields();
            PropertyInfo[] properties = customSettings.GetType().GetProperties();

            var atribut = new SettingsProperty("", true);
            foreach (FieldInfo field in fields)
            {
                if (field.IsDefined(typeof(SettingsProperty)))
                {
                    atribut = (SettingsProperty)field.GetCustomAttribute(typeof(SettingsProperty), false);
                    if (atribut.Temporary)
                        settings.Add(atribut.Key, field.GetValue(null));
                }
            }

            foreach (PropertyInfo property in properties)
            {
                if (property.IsDefined(typeof(SettingsProperty)))
                {
                    atribut = (SettingsProperty)property.GetCustomAttribute(typeof(SettingsProperty), false);
                    if (atribut.Temporary)
                        settings.Add(atribut.Key, property.GetValue(null));
                }
            }

            return settings;
        }

        #endregion Public functions
    }
}