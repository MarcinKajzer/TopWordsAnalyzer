namespace TopWordsAnalyzer.Readers;

public interface ITopWordsService
{
    IResult AnalyzeFile(ReportFormData reportFormData);
}
