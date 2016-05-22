using StreamDownloaderDownload.Hosts;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace StreamDownloaderDownload.Download.JSON
{
    [DataContract]
    public class DownloadData
    {
        public DownloadData()
        {
            this.Progress = new DownloadProgress();
        }

        #region C# variables

        public Host FileHost { get; set; }

        public Hashtable TempHostSettings { get; set; }

        #endregion C# variables

        #region JSON data members

        [DataMember(Name = "raw_url", IsRequired = true)]
        public string RawURL { get; set; } = string.Empty;

        [DataMember(Name = "source_url", IsRequired = true)]
        public string SourceURL { get; set; } = string.Empty;

        [DataMember(Name = "download_destination", IsRequired = true)]
        public string DownloadDestination { get; set; } = string.Empty;

        [DataMember(Name = "temp_file_destination", IsRequired = true)]
        public string TempFileDestination { get; set; } = string.Empty;

        [DataMember(Name = "filename", IsRequired = true)]
        public string FileName { get; set; } = string.Empty;

        [DataMember(Name = "full_filename", IsRequired = true)]
        public string FullFileName { get; set; } = string.Empty;

        [DataMember(Name = "file_extension", IsRequired = true)]
        public string FileExtension { get; set; } = string.Empty;

        [DataMember(Name = "download_status", IsRequired = true)]
        public DownloadProgress Progress { get; set; }

        [DataMember(Name = "host_custom_settings", IsRequired = false)]
        public HashSet<SerializableHostSettings> SerializibleHostSettings { get; set; }

        #endregion JSON data members

        public void SerializeDownloadState(string outputFolder)
        {
            var path = $"{outputFolder}\\SAVE\\{FileName} - {Path.GetFileName(TempFileDestination)}.pdi";

            if (!File.Exists(path))
                using (File.Create(path))
                { }

            using (var downloadRestoreFileStream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(DownloadData));
                jsonSerializer.WriteObject(downloadRestoreFileStream, this);
            }
        }

        public static DownloadData DeserializeDownloadData(string tempFile)
        {
            return DeserializeDownloadData(new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        public static DownloadData DeserializeDownloadData(FileStream stream)
        {
            return (DownloadData)new DataContractJsonSerializer(typeof(DownloadData)).ReadObject(stream);
        }

        public class DownloadProgress
        {
            [DataMember(Name = "Status", IsRequired = true)]
            public int Status { get; set; } = (int)Download.DownloadStatus.NOT_INITIALIZED;

            [DataMember(Name = "WrittenBytes", IsRequired = true)]
            public ulong WrittenBytes { get; set; } = 0;

            [DataMember(Name = "ContentLength", IsRequired = true)]
            public ulong ContenLength { get; set; } = 0;
        }
    }
}