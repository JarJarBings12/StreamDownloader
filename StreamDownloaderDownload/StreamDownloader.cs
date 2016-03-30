using System;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using StreamDownloaderDownload.Hosters;
using StreamDownloaderDownload.Download;
using StreamDownloaderDownload.FileExtensions;

namespace StreamDownloaderDownload
{
    public class StreamDownloader
    {
        private Hashtable _activeDownloads;

        public HashSet<Hoster> SupportedHosters = new HashSet<Hoster>();

        public HashSet<FileExtension> SupportedFilExtensions = new HashSet<FileExtension>();

        public static string DownloadFolder { get; set; } = Path.Combine(Environment.GetEnvironmentVariable("USERPROFILE"), @"Downloads\");

        public static string DownloadTempFolder { get; set; } = System.IO.Path.GetTempPath();

        public static uint ChunkSize { get; set; } = 32000;

        public static DownloadTask CreateDownload(string fileName, string fileType, string url)
        {
            return CreateDownload(DownloadFolder, DownloadTempFolder, fileName, fileType, url);
        }

        public static DownloadTask CreateDownload(string downloadFolder, string tempDownloadFolder, string fileName, string fileType, string url)
        {
            return CreateDownload(DownloadFolder, DownloadTempFolder, fileName, fileType, url, ChunkSize);
        }

        public static DownloadTask CreateDownload(string downloadFolder, string tempDownloadFolder, string fileName, string fileType, string url, uint chunkSize)
        {
            return new DownloadTask(downloadFolder, tempDownloadFolder, fileName, fileType, url, chunkSize);
        }

        public static DownloadTask CreateDownload(string downloadFolder, string tempDownloadFolder, string fileName, string fileType, string url, uint chunkSize, uint writtenChunks, ulong contentLength)
        {
            return new DownloadTask(downloadFolder, tempDownloadFolder, fileName, fileType, url, chunkSize, writtenChunks, contentLength);
        }

        public void RegisterHost(Hoster host)
        {
            this.SupportedHosters.Add(host);
        }

        public void RegisterFileExtension(FileExtension extension)
        {
            this.SupportedFilExtensions.Add(extension);
        }
    }
}
