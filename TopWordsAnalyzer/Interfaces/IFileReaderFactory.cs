using TopWordsAnalyzer.Enums;

namespace TopWordsAnalyzer.Interfaces;

public interface IFileReaderFactory
{
    IFileReader CreateReader(FileExtension ext);
}
