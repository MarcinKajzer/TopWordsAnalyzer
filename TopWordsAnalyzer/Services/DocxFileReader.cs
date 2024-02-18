using DocumentFormat.OpenXml.Packaging;
using TopWordsAnalyzer.Interfaces;

namespace TopWordsAnalyzer.Services;

public class DocxFileReader : IFileReader
{
    public string Read(Stream stream)
    {
        using (WordprocessingDocument doc = WordprocessingDocument.Open(stream, false))
        {
            var body = doc.MainDocumentPart.Document.Body;
            return body.InnerText;
        }
    }
}
