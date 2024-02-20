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
        //validator
        if (formData.Tresholds is null)
        {
            return Results.BadRequest("No tresholds defined.");
        }

        if (formData.File is null)
        {
            return Results.BadRequest("No file uploaded.");
        }

        int[] tresholds;
        try
        {
            tresholds = JsonConvert.DeserializeObject<int[]>(formData.Tresholds);
        }
        catch (Exception e)
        {
            return Results.BadRequest("Tresholds param should be an array.");
        }

        var ext = Path.GetExtension(formData.File.FileName).Replace(".", "");
        var isExtensionValid = Enum.TryParse<FileExtension>(ext, true, out var extension);

        if (!isExtensionValid)
        {
            return Results.BadRequest("Invalid file extension.");
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
            return Results.BadRequest();
        }

        var topWordsResult = _topWordsCounter.Count(text, tresholds);

        var cacheKey = Guid.NewGuid();
        _memoryCache.Set(cacheKey, topWordsResult, TimeSpan.FromMinutes(30));

        topWordsResult.ReportId = cacheKey;
        return Results.Ok(topWordsResult);
    }

    public IResult AnalyzeText(ReportTextData textData)
    {
        //validator
        if (textData.Tresholds is null)
        {
            return Results.BadRequest("No tresholds defined.");
        }

        if (textData.Text is null)
        {
            return Results.BadRequest("No text provided.");
        }

        var topWordsResult = _topWordsCounter.Count(textData.Text, textData.Tresholds);

        var cacheKey = Guid.NewGuid();
        _memoryCache.Set(cacheKey, topWordsResult, TimeSpan.FromMinutes(30));

        topWordsResult.ReportId = cacheKey;
        return Results.Ok(topWordsResult);
    }

    public IResult GetXlsxReport(string reportId)
    {
        if (!_memoryCache.TryGetValue<Report>(reportId, out var data))
        {
            return Results.BadRequest("Raport wygasł.");
        }

        var excel = _reportGenerator.Generate(data);
        return Results.File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "output.xlsx");
    }
}
