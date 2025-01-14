using System;
using System.Text;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.Content;
using PdfSharpCore.Pdf.Content.Objects;
using PdfSharpCore.Pdf.IO;

namespace DocCentrik.Services
{
    public class PDFProcessor
    {
        /// <summary>
        /// Extracts text from a PDF file using PDFSharpCore.
        /// </summary>
        /// <param name="filePath">The path of the PDF file to process.</param>
        /// <returns>The extracted text as a string.</returns>
        public string ExtractText(string filePath)
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
        /// <param name="content">The content object to process.</param>
        /// <param name="textBuilder">The StringBuilder to append extracted text.</param>
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
    }
}
