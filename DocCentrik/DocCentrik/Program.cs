using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DocCentrik.Config;
using DocCentrik.Services;
using DocCentrik.Utils;

namespace DocCentrik
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Load configuration settings from the JSON file.
            var config = DocCentrik.Config.Config.LoadConfiguration("config.json");
            if (config == null)
            {
                Console.WriteLine("Configuration file not found or invalid.");
                return;
            }

            Console.WriteLine("Starting GDPR Search...");

            // Initialize services.
            var fileProcessor = new FileProcessor(config.TesseractDataPath);
            var logger = new Logger(config.LogDirectory);
            SftpUploader? sftpUploader = null;

            // Check if SFTP is enabled in the configuration.
            if (config.EnableSftp)
            {
                sftpUploader = new SftpUploader(config.SftpServer);
                Console.WriteLine("SFTP uploading is enabled.");
            }
            else
            {
                Console.WriteLine("SFTP uploading is disabled.");
            }

            // Scan for files in the specified folder path.
            var files = fileProcessor.ScanFiles(config.FolderPath, config.SupportedExtensions);

            foreach (var file in files)
            {
                Console.WriteLine($"Processing: {file}");

                try
                {
                    // Extract content from the file.
                    var content = fileProcessor.ExtractContent(file);

                    // Search for matches in the content based on keywords and regex patterns.
                    var matches = fileProcessor.SearchContent(
                        content: content,
                        regexPatterns: config.RegexPatterns.Select(r => (r.Pattern, r.Description)).ToList(),
                        keywords: new List<string>(config.Keywords),
                        searchMode: config.SearchMode
                    );

                    // Log matches or absence of matches.
                    if (matches.Count > 0)
                    {
                        foreach (var match in matches)
                        {
                            logger.LogMatch(file, match.Match, Path.GetExtension(file), match.Source, "N/A");
                        }

                        logger.Log(file, "Match Found");

                        // Upload the file to the SFTP server if enabled.
                        if (config.EnableSftp && sftpUploader != null)
                        {
                            Console.WriteLine($"Uploading {file} to SFTP server...");
                            sftpUploader.UploadFile(file);
                            Console.WriteLine($"File {file} successfully uploaded.");
                        }
                    }
                    else
                    {
                        logger.Log(file, "No Match");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing file {file}: {ex.Message}");
                    logger.Log(file, "Error");
                }
            }

            Console.WriteLine("GDPR Search Completed.");
        }
    }
}
