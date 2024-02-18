namespace TopWordsAnalyzer.Interfaces;

public interface IFileReader
{
    string Read(Stream stream);
}