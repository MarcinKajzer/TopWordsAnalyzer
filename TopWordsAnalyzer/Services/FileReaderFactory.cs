using TopWordsAnalyzer.Enums;
using TopWordsAnalyzer.Interfaces;

namespace TopWordsAnalyzer.Services;

public class FileReaderFactory : IFileReaderFactory
{
    public IFileReader CreateReader(FileExtension ext)
    {
        switch (ext)
        {
            case FileExtension.Txt:
                return new TxtFileReader();
            case FileExtension.Pdf:
                return new PdfFileReader();
            case FileExtension.Docx:
                return new DocxFileReader();
            default:
                return new TxtFileReader();
        }
    }
}
