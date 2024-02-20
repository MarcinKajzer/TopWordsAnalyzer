using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using TopWordsAnalyzer.Enums;
using TopWordsAnalyzer.Interfaces;
using TopWordsAnalyzer.Model;

namespace TopWordsAnalyzer.Services;

public class TopWordsService : ITopWordsService
{
    private readonly IFileReaderFactory _fileReaderFactory;
    private readonly ITopWordsCounter _topWordsCounter;
    private readonly IMemoryCache _memoryCache;
    private readonly IXlsxReportGenerator _reportGenerator;

    public TopWordsService(IFileReaderFactory fileReaderFactory, ITopWordsCounter topWordsCounter, 
        IMemoryCache memoryCache, IXlsxReportGenerator reportGenerator)
    {
        _fileReaderFactory = fileReaderFactory;
        _topWordsCounter = topWordsCounter;
        _memoryCache = memoryCache;
        _reportGenerator = reportGenerator;
    }

    public IResult AnalyzeFile(ReportFileData formData)
    {
        if (!ValidateFileInput(formData, out var tresholds, out var extension))
        {
            return Results.BadRequest("Invalid input data.");
        }

        string text;
        try
        {
            using var stream = formData.File.OpenReadStream();
            var reader = _fileReaderFactory.CreateReader(extension);
            text = reader.Read(stream);
        }
        catch (Exception ex)
        {
            return Results.BadRequest(ex.Message);
        }

        var topWordsResult = _topWordsCounter.Count(text, tresholds);
        _memoryCache.Set(topWordsResult.ReportId.ToString(), topWordsResult, TimeSpan.FromMinutes(30));
        return Results.Ok(topWordsResult);
    }

    private bool ValidateFileInput(ReportFileData formData, out int[] tresholds,  out FileExtension extension)
    {
        tresholds = null;
        extension = FileExtension.Txt;

        if (formData.Tresholds is null || formData.File is null)
        {
            return false;
        }

        try
        {
            tresholds = JsonConvert.DeserializeObject<int[]>(formData.Tresholds);
        }
        catch
        {
            return false;
        }

        var ext = Path.GetExtension(formData.File.FileName).Replace(".", "");
        var isExtensionValid = Enum.TryParse<FileExtension>(ext, true, out extension);
        if (!isExtensionValid)
        {
            return false;
        }

        return true;
    }

    public IResult AnalyzeText(ReportTextData textData)
    {
        if (textData.Tresholds is null || textData.Text is null)
        {
           return Results.BadRequest("Invalid input data.");
        }

        var topWordsResult = _topWordsCounter.Count(textData.Text, textData.Tresholds);
        _memoryCache.Set(topWordsResult.ReportId.ToString(), topWordsResult, TimeSpan.FromMinutes(30));
        return Results.Ok(topWordsResult);
    }

    public IResult GetXlsxReport(string reportId)
    {
        if (!_memoryCache.TryGetValue<Report>(reportId, out var data))
        {
            return Results.BadRequest("Raport wygasł.");
        }

        var excel = _reportGenerator.Generate(data);
        return Results.File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Top_Words_Report.xlsx");
    }
}
