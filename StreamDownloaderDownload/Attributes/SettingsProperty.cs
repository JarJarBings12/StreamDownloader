namespace StreamDownloaderDownload.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
    public class SettingsProperty: System.Attribute
    {
        public string Key;
        public bool Temporary = false;

        public SettingsProperty(string key, bool temporary)
        {
            this.Key = key;
            this.Temporary = temporary;
        }
    }
}