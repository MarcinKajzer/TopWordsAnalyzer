using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using TopWordsAnalyzer.Interfaces;

namespace TopWordsAnalyzer.Readers;

public class PdfFileReader : IFileReader
{
    public string Read(Stream stream)
    {
        using (PdfReader pdfReader = new PdfReader(stream))
        {
            StringBuilder bd = new StringBuilder();

            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                var strategy = new SimpleTextExtractionStrategy();
                var currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                bd.Append(currentText);
            }

            return bd.ToString();
        }
    }
}
