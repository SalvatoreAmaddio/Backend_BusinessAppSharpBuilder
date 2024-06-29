namespace Backend.Utils
{
    /// <summary>
    /// This class holds some information about the developer and the client.
    /// </summary>
    public class SoftwareInfo
    {
        /// <summary>
        /// Gets or sets the name of the developer.
        /// </summary>
        public string DeveloperName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the website of the developer.
        /// </summary>
        public Uri? DeveloperWebsite { get; set; }

        /// <summary>
        /// Gets or sets the year when the software was developed.
        /// </summary>
        public string SoftwareYear { get; set; } = DateTime.Now.Year.ToString();

        /// <summary>
        /// Gets the name of the software. This property is set by <see cref="Sys.AppName"/>.
        /// </summary>
        public string? SoftwareName { get; } = string.Empty;

        /// <summary>
        /// Gets the version of the software. This property is set by <see cref="Sys.AppVersion"/>.
        /// </summary>
        public string? SoftwareVersion { get; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the client this software was developed for.
        /// </summary>
        public string ClientName { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftwareInfo"/> class.
        /// </summary>
        public SoftwareInfo()
        {
            SoftwareName = Sys.AppName;
            SoftwareVersion = $"v. {Sys.AppVersion}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SoftwareInfo"/> class with specified details.
        /// </summary>
        /// <param name="developerName">The name of the developer.</param>
        /// <param name="developerWebsite">The website of the developer.</param>
        /// <param name="client">The name of the client.</param>
        /// <param name="year">The year the software was developed.</param>
        public SoftwareInfo(string developerName, string developerWebsite, string client, string year) : this()
        {
            DeveloperName = developerName;
            DeveloperWebsite = new Uri($"https://{developerWebsite}");
            ClientName = client;
            SoftwareYear = year;
        }
    }

}