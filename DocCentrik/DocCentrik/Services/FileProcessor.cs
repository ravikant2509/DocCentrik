using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using Tesseract;

namespace DocCentrik.Services
{
    /// <summary>
    /// Provides functionality for processing files, including scanning directories, extracting content, and searching for specific patterns or keywords.
    /// </summary>
    public class FileProcessor
    {
        private readonly string _tesseractDataPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileProcessor"/> class with the specified Tesseract data path.
        /// </summary>
        /// <param name="tesseractDataPath">Path to the Tesseract data files.</param>
        public FileProcessor(string tesseractDataPath)
        {
            _tesseractDataPath = tesseractDataPath ?? throw new ArgumentNullException(nameof(tesseractDataPath));
        }

        /// <summary>
        /// Scans the specified directory for files with supported extensions.
        /// </summary>
        /// <param name="folderPath">The directory to scan.</param>
        /// <param name="extensions">Array of supported file extensions.</param>
        /// <returns>A collection of file paths matching the supported extensions.</returns>
        public IEnumerable<string> ScanFiles(string folderPath, string[] extensions)
        {
            return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                            .Where(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Searches the content for matches based on keywords, regex patterns, or both, depending on the configuration.
        /// </summary>
        /// <param name="content">The content to search.</param>
        /// <param name="regexPatterns">List of regex patterns and their descriptions.</param>
        /// <param name="keywords">List of keywords to search for.</param>
        /// <param name="searchMode">The mode of search: "keywords", "regex", or "both".</param>
        /// <returns>A list of matches found in the content.</returns>
        public List<(string Match, string Source)> SearchContent(
            string content,
            List<(string pattern, string description)> regexPatterns,
            List<string> keywords,
            string searchMode)
        {
            var matches = new List<(string Match, string Source)>();

            if (searchMode.Equals("keywords", StringComparison.OrdinalIgnoreCase) || searchMode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var keyword in keywords)
                {
                    if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        matches.Add((keyword, "Keyword"));
                    }
                }
            }

            if (searchMode.Equals("regex", StringComparison.OrdinalIgnoreCase) || searchMode.Equals("both", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var (pattern, description) in regexPatterns)
                {
                    var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                    var matchCollection = regex.Matches(content);

                    foreach (Match match in matchCollection)
                    {
                        matches.Add((match.Value, $"Regex ({description})"));
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Extracts content from a supported file type.
        /// </summary>
        /// <param name="filePath">The file path of the document to process.</param>
        /// <returns>The extracted content as a string, or an empty string if extraction fails.</returns>
        public string ExtractContent(string filePath)
        {
            try
            {
                if (filePath.EndsWith(".txt") || filePath.EndsWith(".log") || filePath.EndsWith(".csv") || filePath.EndsWith(".err"))
                {
                    return File.ReadAllText(filePath);
                }

                if (filePath.EndsWith(".pdf"))
                {
                    return ExtractFromPdf(filePath);
                }

                if (filePath.EndsWith(".docx") || filePath.EndsWith(".doc"))
                {
                    return ExtractFromWord(filePath);
                }

                if (filePath.EndsWith(".xlsx") || filePath.EndsWith(".xls"))
                {
                    return ExtractFromExcel(filePath);
                }

                if (filePath.EndsWith(".ppt") || filePath.EndsWith(".pptx"))
                {
                    return ExtractFromPowerPoint(filePath);
                }

                if (IsImage(filePath))
                {
                    return ExtractFromImage(filePath);
                }

                throw new NotSupportedException($"The file format is not supported: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting content from {filePath}: {ex.Message}");
                return string.Empty;
            }
        }


        /// <summary>
        /// Extracts text content from a PDF file.
        /// </summary>
        private string ExtractFromPdf(string filePath)
        {
            try
            {
                using var pdfReader = new iText.Kernel.Pdf.PdfReader(filePath);
                using var pdfDocument = new iText.Kernel.Pdf.PdfDocument(pdfReader);

                var text = new StringBuilder();
                var strategy = new iText.Kernel.Pdf.Canvas.Parser.Listener.SimpleTextExtractionStrategy();

                for (int page = 1; page <= pdfDocument.GetNumberOfPages(); page++)
                {
                    var pdfPage = pdfDocument.GetPage(page);
                    var content = iText.Kernel.Pdf.Canvas.Parser.PdfTextExtractor.GetTextFromPage(pdfPage, strategy);
                    text.Append(content);
                }

                return text.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from PDF: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts text content from a Word document.
        /// </summary>
        private string ExtractFromWord(string filePath)
        {
            try
            {
                using var wordDocument = WordprocessingDocument.Open(filePath, false);
                var body = wordDocument.MainDocumentPart?.Document.Body;

                return body != null ? body.InnerText : string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from Word: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts text content from an Excel file.
        /// </summary>
        private string ExtractFromExcel(string filePath)
        {
            try
            {
                using var workbook = new ClosedXML.Excel.XLWorkbook(filePath);
                var extractedContent = new StringBuilder();

                foreach (var worksheet in workbook.Worksheets)
                {
                    extractedContent.AppendLine($"Worksheet: {worksheet.Name}");

                    foreach (var row in worksheet.RowsUsed())
                    {
                        foreach (var cell in row.CellsUsed())
                        {
                            extractedContent.Append($"{cell.Value} ");
                        }
                        extractedContent.AppendLine();
                    }

                    extractedContent.AppendLine();
                }

                return extractedContent.ToString().Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from Excel: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts text content from an image using OCR.
        /// </summary>
        private string ExtractFromImage(string filePath)
        {
            try
            {
                using var engine = new TesseractEngine(_tesseractDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(filePath);
                using var page = engine.Process(img);

                return page.GetText().Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text from image: {ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// Extracts text content from PowerPoint files (.ppt and .pptx).
        /// </summary>
        /// <param name="filePath">The PowerPoint file path.</param>
        /// <returns>The extracted text content.</returns>
        private string ExtractFromPowerPoint(string filePath)
        {
            try
            {
                using var presentation = PresentationDocument.Open(filePath, false);
                var slides = presentation.PresentationPart.SlideParts;
                var text = string.Join("\n", slides.SelectMany(slide => slide.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>().Select(t => t.Text)));
                return text;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting content from PowerPoint file {filePath}: {ex.Message}");
                return string.Empty;
            }
        }
        /// <summary>
        /// Determines whether the specified file is an image based on its extension.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>True if the file is an image, otherwise false.</returns>
        public bool IsImage(string filePath)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif" };
            return imageExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }
    }
}
