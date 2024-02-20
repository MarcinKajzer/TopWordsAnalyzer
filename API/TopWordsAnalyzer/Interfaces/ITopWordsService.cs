using TopWordsAnalyzer.Model;

namespace TopWordsAnalyzer.Interfaces;

public interface ITopWordsService
{
    IResult AnalyzeFile(ReportFileData reportFormData);
    IResult AnalyzeText(ReportTextData reportFormData);
    IResult GetXlsxReport(string reportId);
}
