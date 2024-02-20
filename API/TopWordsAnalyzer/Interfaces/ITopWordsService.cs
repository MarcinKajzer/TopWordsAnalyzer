namespace TopWordsAnalyzer.Interfaces;

public interface ITopWordsService
{
    IResult AnalyzeFile(ReportFormData reportFormData);
}
