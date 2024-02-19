using TopWordsAnalyzer.Interfaces;

namespace TopWordsAnalyzer.Readers;

public class TxtFileReader : IFileReader
{
    public string Read(Stream stream)
    {
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
