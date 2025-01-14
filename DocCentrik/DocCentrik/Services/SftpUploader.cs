using System;
using System.IO;
using Renci.SshNet;
using DocCentrik.Config;

namespace DocCentrik.Services
{
    /// <summary>
    /// Handles the uploading of files to an SFTP server.
    /// </summary>
    public class SftpUploader
    {
        private readonly SftpServerConfig _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="SftpUploader"/> class with the specified SFTP server configuration.
        /// </summary>
        /// <param name="config">The SFTP server configuration.</param>
        public SftpUploader(SftpServerConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Uploads a specified file to the SFTP server.
        /// </summary>
        /// <param name="filePath">The local file path of the file to upload.</param>
        public void UploadFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            try
            {
                using var sftp = new SftpClient(_config.Host, _config.Port, _config.Username, _config.Password);
                sftp.Connect();

                using var fileStream = File.OpenRead(filePath);
                sftp.UploadFile(fileStream, Path.GetFileName(filePath));

                Console.WriteLine($"Successfully uploaded file: {Path.GetFileName(filePath)}");
                sftp.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading file to SFTP server: {ex.Message}");
                throw;
            }
        }
    }
}
