namespace OwncloudUniversal.OwnCloud.Model
{
    public class ServerStatus
    {
        public bool Installed { get; set; }
        public bool Maintenance { get; set; }
        public string Version { get; set; }
        public string Versionstring { get; set; }
        public string Edition { get; set; }
        public string ResponseCode { get; set; }
    }
}
