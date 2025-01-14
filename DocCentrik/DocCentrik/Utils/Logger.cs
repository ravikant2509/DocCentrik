using System;
using System.IO;

namespace DocCentrik.Utils
{
    /// <summary>
    /// Provides logging functionality for application events and match reports.
    /// </summary>
    public class Logger
    {
        private readonly string _logDirectory;
        private readonly string _logFileName;
        private readonly string _matchReportFileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// Ensures that the log and match report directories exist.
        /// </summary>
        /// <param name="logDirectory">Directory to store log files.</param>
        public Logger(string logDirectory)
        {
            if (string.IsNullOrEmpty(logDirectory))
                throw new ArgumentNullException(nameof(logDirectory));

            _logDirectory = logDirectory;

            // Ensure directory exists
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            // Generate daily log file names
            var date = DateTime.Now.ToString("yyyy-MM-dd");
            _logFileName = Path.Combine(_logDirectory, $"DocCentrikLog_{date}.csv");
            _matchReportFileName = Path.Combine(_logDirectory, $"DocCentrikMatchReport_{date}.csv");

            // Initialize log files
            InitializeLogFiles();
        }

        /// <summary>
        /// Logs a message to the general log file.
        /// </summary>
        /// <param name="filePath">The file path being processed.</param>
        /// <param name="status">The status of the processing (e.g., "Match Found", "No Match", "Error").</param>
        public void Log(string filePath, string status)
        {
            string logEntry = $"{DateTime.Now},{filePath},{status}";
            File.AppendAllLines(_logFileName, new[] { logEntry });
        }

        /// <summary>
        /// Logs a match entry to the match report file.
        /// </summary>
        /// <param name="filePath">The file path where the match was found.</param>
        /// <param name="matchingWord">The word or pattern that matched.</param>
        /// <param name="fileType">The type of the file (e.g., ".txt", ".pdf").</param>
        /// <param name="matchSource">The source of the match (e.g., "Keyword", "Regex").</param>
        /// <param name="ocrStatus">Indicates whether OCR was used (e.g., "OCR", "Non-OCR").</param>
        public void LogMatch(string filePath, string matchingWord, string fileType, string matchSource, string ocrStatus)
        {
            string reportEntry = $"{DateTime.Now},{filePath},{matchingWord},{fileType},{matchSource},{ocrStatus}";
            File.AppendAllLines(_matchReportFileName, new[] { reportEntry });
        }

        /// <summary>
        /// Ensures log and match report files exist and are initialized with headers if necessary.
        /// </summary>
        private void InitializeLogFiles()
        {
            if (!File.Exists(_logFileName))
            {
                File.WriteAllText(_logFileName, "Timestamp,File Name,Status\n");
            }

            if (!File.Exists(_matchReportFileName))
            {
                File.WriteAllText(_matchReportFileName, "Timestamp,File Name,Matching Word,File Type,Match Found In,OCR or Non-OCR\n");
            }
        }
    }
}
