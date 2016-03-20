using System.Runtime.Serialization;

namespace StreamDownloaderDownload.Download.JSON
{
    [DataContract]
    public class StoredDownload
    {
        [DataMember(Name = "DownloadTempFolder")]
        public string TempFolder { get; set; }
        [DataMember(Name = "FileName")]
        public string FileName { get; set; }
        [DataMember(Name = "FileType")]
        public string FileType { get; set; }
        [DataMember(Name = "DownloadFolder")]
        public string DownloadFolder { get; set; }
        [DataMember(Name = "Source")]
        public string Source { get; set; }

        [DataMember(Name = "DownloadStatus")]
        public DownloadStatus DownloadStatus { get; set; }
    }

    [DataContract]
    public class DownloadStatus
    {
        [DataMember(Name = "WrittenChunks")]
        public uint WrittenChunks { get; set; }
        [DataMember(Name = "ChunkSize")]
        public uint ChunkSize { get; set; }
        [DataMember(Name = "ContentLength")]
        public ulong ContentLength { get; set; }
    }
}
