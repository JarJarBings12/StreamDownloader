using System.Runtime.Serialization;

namespace StreamDownloaderDownload.Download.JSON
{
    [DataContract]
    public struct SerializableHostSettings
    {
        [DataMember(Name = "Key", IsRequired = true)]
        public string Key { get; set; }

        [DataMember(Name = "Value", IsRequired = true)]
        public object Value { get; set; }

        public SerializableHostSettings(string key, object value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}