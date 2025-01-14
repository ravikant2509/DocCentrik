using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using Tesseract;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Content;
using PdfSharpCore.Pdf.Content.Objects;
using PdfSharpCore.Pdf.IO;

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
        public IEnumerable<string> ScanFiles(string folderPath, string[] extensions)
        {
            return Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories)
                            .Where(file => extensions.Any(ext => file.EndsWith(ext, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Extracts content from a supported file type.
        /// </summary>
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
                    return ExtractFromPdf(filePath); // Updated method
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
        /// Extracts text content from a PDF file using PDFSharpCore.
        /// </summary>
        private string ExtractFromPdf(string filePath)
        {
            try
            {
                using var document = PdfReader.Open(filePath, PdfDocumentOpenMode.ReadOnly);
                var text = new StringBuilder();

                foreach (var page in document.Pages)
                {
                    var content = ContentReader.ReadContent(page);
                    ExtractTextFromContent(content, text);
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
        /// Recursively extracts text from PDF content objects.
        /// </summary>
        private void ExtractTextFromContent(CObject content, StringBuilder textBuilder)
        {
            if (content is COperator cOperator && cOperator.Operands != null)
            {
                foreach (var operand in cOperator.Operands)
                {
                    ExtractTextFromContent(operand, textBuilder);
                }
            }
            else if (content is CSequence cSequence)
            {
                foreach (var element in cSequence)
                {
                    ExtractTextFromContent(element, textBuilder);
                }
            }
            else if (content is CString cString)
            {
                textBuilder.Append(cString.Value);
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
        /// Extracts text content from PowerPoint files (.ppt and .pptx).
        /// </summary>
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
        /// Determines whether the specified file is an image based on its extension.
        /// </summary>
        public bool IsImage(string filePath)
        {
            var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".tiff", ".tif" };
            return imageExtensions.Any(ext => filePath.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Searches the content for matches based on keywords, regex patterns, or both.
        /// </summary>
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
    }
}
