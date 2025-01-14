using System.Text.Json;

namespace DocCentrik.Config
{
    /// <summary>
    /// Represents the application configuration, including folder paths, keywords, regex patterns,
    /// SFTP server details, and other settings.
    /// </summary>
    public class Config
    {
        public string FolderPath { get; set; } = string.Empty;
        public string[] Keywords { get; set; } = Array.Empty<string>();
        public List<RegexPattern> RegexPatterns { get; set; } = new List<RegexPattern>();
        public string SearchMode { get; set; } = "both";
        public string[] SupportedExtensions { get; set; } = Array.Empty<string>();
        public SftpServerConfig SftpServer { get; set; } = new SftpServerConfig();
        public string OutputLogPath { get; set; } = string.Empty;
        public string MatchReportPath { get; set; } = string.Empty;
        public string LogDirectory { get; set; } = string.Empty;
        public string TesseractDataPath { get; set; } = string.Empty;

        // New parameter to enable or disable SFTP uploading.
        public bool EnableSftp { get; set; } = false;

        /// <summary>
        /// Loads configuration settings from a specified JSON file.
        /// </summary>
        /// <param name="path">Path to the configuration file.</param>
        /// <returns>An instance of <see cref="Config"/> populated with settings, or null if the file is invalid.</returns>
        public static Config? LoadConfiguration(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Config>(json);
        }
    }

    /// <summary>
    /// Represents a regex pattern and its description for identifying specific content.
    /// </summary>
    public class RegexPattern
    {
        public string Pattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Contains SFTP server configuration details such as host, port, and credentials.
    /// </summary>
    public class SftpServerConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 22;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
