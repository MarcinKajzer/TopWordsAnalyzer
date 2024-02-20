using TopWordsAnalyzer.Model;

namespace TopWordsAnalyzer.Interfaces;

public interface IXlsxReportGenerator
{
    Stream Generate(Report data);
}
