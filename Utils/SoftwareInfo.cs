namespace Backend.Utils
{
    /// <summary>
    /// This class holds some information about the Developer and the Client.
    /// </summary>
    public class SoftwareInfo
    {
        public string DeveloperName { get; set; } = string.Empty;
        public Uri? DeveloperWebsite { get; set; }
        /// <summary>
        /// When the Software was Developed
        /// </summary>
        public string SoftwareYear { get; set; } = DateTime.Now.Year.ToString();
        
        /// <summary>
        /// This property is set by the <see cref="Sys.AppName"/>.
        /// </summary>
        public string? SoftwareName { get; } = string.Empty;

        /// <summary>
        /// This property is set by the <see cref="Sys.AppVersion"/>.
        /// </summary>
        public string? SoftwareVersion { get; } = string.Empty;
        
        /// <summary>
        /// The name of the client this software was developed for.
        /// </summary>
        public string ClientName { get; set;} = string.Empty;
        
        public SoftwareInfo() 
        {
            SoftwareName = Sys.AppName;
            SoftwareVersion = $"v. {Sys.AppVersion}";
        }

        public SoftwareInfo(string developerName, string developerWebsite, string client, string year) : this()
        {
            DeveloperName = developerName;
            DeveloperWebsite = new Uri($"https://{developerWebsite}");
            ClientName = client;
            SoftwareYear = year;
        }
    }
}